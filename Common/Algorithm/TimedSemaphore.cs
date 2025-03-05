using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace K10
{
    public class TimedSemaphore : Semaphore
    {
        private static string GetLockID(int id) => $"__timeLock{id}";

        private readonly Dictionary<int, Coroutine> lockCoroutines = new();
        private int currentTimeId;

        private readonly StartCoroutineReference StartCoroutine;
        private readonly StopCoroutineReference StopCoroutine;

        public int TempBlockCount => lockCoroutines.Count;
        public bool IsPermanentlyLocked => BlockCount - TempBlockCount > 0;

        public void BlockTemporarily(float cooldownSeconds)
        {
            var coroutine = StartCoroutine(LockCoroutine(cooldownSeconds, currentTimeId));
            lockCoroutines.Add(currentTimeId, coroutine);

            currentTimeId++;
        }

        public override void Clear()
        {
            base.Clear();

            foreach (var coroutine in lockCoroutines.Values)
                StopCoroutine(coroutine);

            lockCoroutines.Clear();
        }

        private IEnumerator LockCoroutine(float cooldownSeconds, int id)
        {
            var lockID = GetLockID(id);
            Block(lockID);

            yield return new WaitForSeconds(cooldownSeconds);

            Release(lockID);
            lockCoroutines.Remove(id);
        }

        public TimedSemaphore(StartCoroutineReference startFunction, StopCoroutineReference stopFunction)
        {
            StartCoroutine = startFunction;
            StopCoroutine = stopFunction;
        }
    }
}