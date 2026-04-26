using System;
using System.Collections.Generic;

namespace K10.Promises
{
	public interface IFutureBase 
	{ 
		bool IsComplete { get; } 
	}

	public interface IFutureObserver : IFutureBase
	{
		void Register( Action listener );
	}

	public interface IFutureObserver<Result> : IFutureBase
	{
		void Register( Action<Result> listener );
	}

	public interface IFuture : IFutureObserver
	{
		void ForceComplete();
	}

	public interface IFuture<Result> : IFutureObserver<Result>
	{
		void ForceComplete( Result t );
	}

	public class Future : IFuture
	{
		bool _isComplete = false;

		List<Action> _onCompletion = null;

		public void Register( Action listener )
		{
			if( listener == null ) return;
			if( _isComplete ) 
			{
				listener();
				return;
			}
			if( _onCompletion == null ) _onCompletion = new();
			_onCompletion.Add( listener );
		}

		public void ForceComplete()
		{
			if( _isComplete )
				return;

			_isComplete = true;

			if( _onCompletion == null ) return;

			foreach( var act in _onCompletion ) act();
			_onCompletion.Clear();
			_onCompletion = null;
		}

		public bool IsComplete { get { return _isComplete; } }
	}

	public class Future<Result> : IFuture<Result>
	{
		bool _isComplete = false;
		Result _result;

		List<Action<Result>> _onCompletion;
        public bool IsComplete => _isComplete;

		public void Reset()
		{
			_isComplete = false;
		}

        public void Register( Action<Result> listener )
		{
			if( listener == null ) return;
			if( _isComplete ) 
			{
				listener( _result );
				return;
			}
			if( _onCompletion == null ) _onCompletion = new();
			_onCompletion.Add( listener );
		}

		public void ForceComplete( Result result )
		{
			if( _isComplete ) return;

			_isComplete = true;
			_result = result;

			if( _onCompletion == null ) return;

			foreach( var act in _onCompletion ) act( _result );
			_onCompletion.Clear();
			_onCompletion = null;
		}
	}
}