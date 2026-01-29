using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;

public class AggregatedPredictor<T>
{
    public readonly List<SubsetSelectorPredictor<T>> nestedCrawlers = new();

    public readonly RangeSummary Score = new();
    public readonly RangeSummary ElementsCount = new();

    public void Calculate( IAggregatedSubsetSelector<T> agg, Func<T,int> Scorer = null )
    {
        nestedCrawlers.Clear();

        Score.SetZero();
        ElementsCount.SetZero();

        Debug.Log( $"ElementsCount:{ElementsCount}" );

        if( agg == null ) return;

        for ( int i = 0; i < agg.Count; i++ )
        {
            var crawler = new SubsetSelectorPredictor<T>();
            crawler.SetScores( Scorer );
            crawler.Calculate( agg.GetEntry(i) );
            nestedCrawlers.Add( crawler );

            Score.Combine( crawler.Score );
            ElementsCount.Combine( crawler.ElementsCount );
            
            Debug.Log( $"Jamming {i}: {crawler}" );
            Debug.Log( $"ElementsCount:{ElementsCount}" );
        }

    }
}

public class RangeSummary
{
    public double Min = double.MaxValue;
    public double Average = 0;
    public double Max = double.MinValue;
    double chancesSum = 0;

    int combines = 0;
    bool WrongSum => chancesSum > ( combines * 1.05 ) || chancesSum < ( combines * .95 );

    public void Clear()
    {
        Min = double.MaxValue;
        Average = 0;
        Max = double.MinValue;
        chancesSum = 0;
        combines = 1;
    }

    public void StartAccumulator()
    {
        Min = double.MaxValue;
        Average = 0;
        Max = double.MinValue;
        chancesSum = 0;
        combines = 1;
    }

    public void SetZero()
    {
        Min = 0;
        Average = 0;
        Max = 0;
        chancesSum = 0;
        combines = 0;
    }

    public void RegisterValue( double value, double chance )
    {
        Debug.Log( $"{this}.RegisterValue( {value}, {chance*100:N2}% )" );
        Min = Math.Min( value, Min );
        Average += value * chance;
        Max = Math.Max( value, Max );
        chancesSum += chance;
        Debug.Log( $"= {this}" );
    }

    public void RegisterValue( RangeSummary value, double chance )
    {
        Min = Math.Min( value.Min, Min );
        Average += value.Average * chance;
        Max = Math.Max( value.Max, Max );
        chancesSum += chance;
    }

    public void Combine( RangeSummary range )
    {
        Min += range.Min;
        Average += range.Average;
        Max += range.Max;
        chancesSum += range.chancesSum;
        combines++;
    }

    public override string ToString() => $"[ {Min:N0} ... {Average:N1} ... {Max:N0} ]{(WrongSum?"!WRONG!":"")}";
}

public class SubsetSelectorPredictor<T>
    {
        public T[] _elements;
        public int[] _countCache;
        public float[] _rollChancesCache;
        public int[] _minCache;
        public int[] _maxCache;
        public float[] _chancesCache;
        // public int[] _scores;
        Func<T,int> _scorer;
        public string[] _namesCache;
        float _sumWeight;
        
        double[,,] M;
        double[] acc;

        public int rMin;
        public int rMax;
        int count;
        int maxElementsCount;
        int minSum;

        public readonly List<int[]> combinations = new();
        public readonly List<double> combinationsChances = new();

        public readonly RangeSummary Score = new();
        public readonly RangeSummary ElementsCount = new();

        List<(string name, float percentage)> _permutations = new();
        Dictionary<string, float> _permutationChances = new();

        public override string ToString()
        {
            return $"Score:{Score} ElementsCount:{ElementsCount} _elements:{_elements.ToStringOrNull()}\ncombinations:{combinations.ToStringOrNull()}\ncombinationsChances:{combinationsChances.ToStringOrNull()}\n_permutations:{_permutations.ToStringOrNull()}";
        }

        public void SetScores( Func<T,int> scorer )
        {
            _scorer = scorer;
        }

        public void Calculate( ISubsetSelector<T> set )
        {
            if( set == null ) return;
		    count = set.EntriesCount;
            FillElements(set);
            FillCaches(set);
            CalculatePredictions(set);
        }

        private void FillElements(ISubsetSelector<T> set)
        {
            var count = set.EntriesCount;
            if (_elements == null || _elements.Length != count) _elements = new T[count];
            for (int i = 0; i < set.EntriesCount; i++)
            {
                _elements[i] = set.GetEntry(i).Element;
            }
        }

        public void FillCaches( ISubsetSelector set )
        {
            FillEntriesCaches(set);
            FillRangeWeights(set);
        }

        private void FillRangeWeights(ISubsetSelector set)
        {
            var rCount = set.Max + 1 - set.Min;
            if( !set.IsBiased ) 
            {
                _rollChancesCache = null;
                return;
            }
            else if (_rollChancesCache == null || _rollChancesCache.Length != rCount) _rollChancesCache = new float[rCount];

            for (int i = 0; i < rCount; i++)
            {
                _rollChancesCache[i] = set.GetBiasWeight(set.Min + i);
            }
        }

        public void FillEntriesCaches( ISubsetSelector set )
        {
            var count = set.EntriesCount;

            if (_minCache == null || _minCache.Length != count) _minCache = new int[count];
            if (_maxCache == null || _maxCache.Length != count) _maxCache = new int[count];
            if (_namesCache == null || _namesCache.Length != count) _namesCache = new string[count];
            if (_chancesCache == null || _chancesCache.Length != count) _chancesCache = new float[count];

            _sumWeight = 0f;

            for( int i = 0; i < count; i++ )
            {
                var element = set.GetEntryObject( i );
                _sumWeight += element.Weight;
            }

            minSum = 0;
            maxElementsCount = 0;

            for (int i = 0; i < count; i++)
            {
                var element = set.GetEntryObject( i );
                _chancesCache[i] = element.Weight / _sumWeight;
                var min = element.Guaranteed;
                _minCache[i] = min;
                var max =  element.Cap;
                _maxCache[i] = max;
                var objRef = element.ElementAsObject;
                _namesCache[i] = objRef.DebugNameOrNull();
                minSum += min;
                
                var realCap = Mathf.Max( min, max );
                maxElementsCount = Mathf.Max( maxElementsCount, realCap );
            }

            _rollChancesCache = null;
        }

        public void CalculatePredictions( ISubsetSelector set )
        {
            int count = set.EntriesCount;

            float sumWeight = 0f;

            for( int i = 0; i < count; i++ )
            {
                var element = set.GetEntryObject(i);
                sumWeight += element.Weight;
            }

            CalculatePredictions( set.Min, set.Max, sumWeight );
        }

        public void CalculatePredictions( int minRoll, int maxRoll, float sumWeight )
        {
            rMin = Mathf.Max( minRoll, 0 );
            rMax = Mathf.Max( maxRoll, 0 );

            var maxElementsCountPossible = Mathf.Max( maxElementsCount, rMax );

            M = new double[count, maxElementsCountPossible + 1, rMax + 1];
            CalculateBacktracking( count, _minCache, _maxCache, _chancesCache, rMax, maxElementsCountPossible, M );

            acc = new double[rMax + 1];
            CalculateAcc( count, rMax, maxElementsCountPossible, M, acc );
            
            Permutations();
            _permutations.Sort(PermutationSorting);
        }
        

        private static void CalculateAcc( int count, int rMax, int maxElements, double[,,] M, double[] acc )
        {
            for( int i = 0; i < count; i++ )
            {
                for( int k = 1; k <= rMax; k++ )
                {
                    for( int j = 0; j <= maxElements; j++ )
                    {
                        acc[k] += M[i, j, k];
                    }
                }
            }
        }

        private static void CalculateBacktracking( int count, int[] min, int[] max, float[] chances, int rMax, int maxElements, double[,,] Predictions )
        {
            var pool = new List<(int id, int max, float chance)>();
            var sequence = new List<int>();
            var elements = new int[count];
            var total = 0;

            var basePercentage = 1f;
            for( int i = 0; i < count; i++ )
            {
                var g = min[i];
                elements[i] = g;
                total += g;
                for( int j = 0; j < g; j++ ) sequence.Add( i );
                var chance = chances[i];
                var addToPool = ( ( max[i] == 0 || g < max[i] ) && chance > 0 );
                if( addToPool ) pool.Add( (i, max[i], chance) );
                else basePercentage -= chance;
            }

            RebasePoolElements( basePercentage, pool );

            for( int i = 0; i <= total && i <= rMax; i++ )
            {
                for( int j = 0; j < count; j++ )
                {
                    var c = elements[j];
                    Predictions[j, c, i] = 1;
                }
            }

            TryExpand( 1, total, rMax, elements, sequence, pool, Predictions );
        }

        static void RebasePoolElements( float newBase, List<(int id, int max, float chance)> pool )
        {
            for( int i = 0; i < pool.Count; i++ )
            {
                var element = pool[i];
                element.chance /= newBase;
                pool[i] = element;
            }
        }

        static void TryExpand( double chance, int total, int maxTotal, int[] elements, List<int> sequence, List<(int id, int max, float chance)> pool, double[,,] M )
        {
            var newTotal = total + 1;
            if( newTotal > maxTotal ) return;

            for( int i = 0; i < pool.Count; i++ )
            {
                var e = pool[i];

                var newCount = elements[e.id] + 1;
                elements[e.id] = newCount;
                var removeFromPool = e.max != 0 && newCount >= e.max;
                if( removeFromPool )
                {
                    pool.RemoveAt( i );
                    RebasePoolElements( 1 - e.chance, pool );
                }

                chance *= e.chance;
                sequence.Add( e.id );
                // var countStr = string.Join( ", ", System.Array.ConvertAll<int, string>( sequence.ToArray(), ( int v ) => v.ToString() ) );
                // Debug.Log( $"{{ {countStr} }}[{sequence.Count}] => {( chance * 100 ):N1}%" );

                for( int j = 0; j < elements.Length; j++ )
                {
                    var c = elements[j];
                    M[j, c, newTotal] += chance;
                }

                TryExpand( chance, newTotal, maxTotal, elements, sequence, pool, M );

                sequence.RemoveAt( sequence.Count - 1 );
                chance /= e.chance;
                elements[e.id] = newCount - 1;
                if( removeFromPool )
                {
                    RebasePoolElements( 1 / ( 1 - e.chance ), pool );
                    pool.Insert( i, e );
                }
            }
        }

        public float GetTableHeight()
        {
            return ( count + 1 ) *( EditorGUIUtility.singleLineHeight + 3 );
        }

        public void DrawTable( Rect rect )
        {
            var lh = EditorGUIUtility.singleLineHeight;

            int minE = int.MaxValue;
            int maxE = int.MinValue;

            for( int k = rMin; k <= rMax; k++ )
            {
                for( int i = 0; i < count; i++ )
                {
                    for( int j = 0; j <= maxElementsCount; j++ )
                    {
                        if( Mathf.Approximately( (float)M[i, j, k], 0 ) ) continue;
                        minE = Mathf.Min( j, minE );
                        maxE = Mathf.Max( j, maxE );
                    }
                }
            }

            var slices = ( maxE - minE ) + 2;
            if( rMin <= rMax )
            {
                var toplineRect = rect.GetLineTop( lh + 3 );
                GUI.Label( toplineRect.VerticalSlice( 0, slices ), "Average", GUI.skin.box );
                for( int j = minE; j <= maxE; j++ ) GUI.Label( toplineRect.VerticalSlice( ( j - minE ) + 1, slices ), $"{j}", GUI.skin.box );
                
                var range = Mathf.Max( rMax - rMin, 0 ) + 1;
                float sumWeight = range;
                
                if (_rollChancesCache != null && _rollChancesCache.Length >= range)
                {
                    sumWeight = 0;
                    for (int i = 0; i < range; i++)
                    {
                        sumWeight += _rollChancesCache[i];
                    }
                }

                for( int i = 0; i < count; i++ )
                {
                    var lineRect = rect.GetLineTop( lh );
                    GUI.Label( lineRect.VerticalSlice( 0, slices ), _namesCache[i] );
                    for( int j = minE; j <= maxE; j++ )
                    {
                        double val = 0;
                        for (int k = rMin; k <= rMax; k++)
                        {
                            var chance = M[i, j, k];
                            if (_rollChancesCache != null && k - rMin < _rollChancesCache.Length) chance *= _rollChancesCache[k - rMin];
                            val += chance;
                        }
                        val /= sumWeight;
                        val = System.Math.Max( val, 0 );

                        var r = lineRect.VerticalSlice( ( j - minE ) + 1, slices );
                        GUIProgressBar.Draw( r, (float)val );
                    }
                }
            }
        }

        public void DrawTable()
        {
            var lh = EditorGUIUtility.singleLineHeight;

            int minE = int.MaxValue;
            int maxE = int.MinValue;

            for( int k = rMin; k <= rMax; k++ )
            {
                for( int i = 0; i < count; i++ )
                {
                    for( int j = 0; j <= maxElementsCount; j++ )
                    {
                        if( Mathf.Approximately( (float)M[i, j, k], 0 ) ) continue;
                        minE = Mathf.Min( j, minE );
                        maxE = Mathf.Max( j, maxE );
                    }
                }
            }

            var slices = ( maxE - minE ) + 2;
            if( rMin <= rMax )
            {
                var rect = EditorGUILayout.BeginHorizontal( GUILayout.Height( lh + 3 ) );
                rect.RequestTop( lh + 3 );
                GUILayout.Space( lh + 3 );
                GUI.Label( rect.VerticalSlice( 0, slices ), "Average", GUI.skin.box );
                for( int j = minE; j <= maxE; j++ ) GUI.Label( rect.VerticalSlice( ( j - minE ) + 1, slices ), $"{j}", GUI.skin.box );
                EditorGUILayout.EndHorizontal();
                
                var range = Mathf.Max( rMax - rMin, 0 ) + 1;
                float sumWeight = range;
                
                if (_rollChancesCache != null && _rollChancesCache.Length >= range)
                {
                    sumWeight = 0;
                    for (int i = 0; i < range; i++)
                    {
                        sumWeight += _rollChancesCache[i];
                    }
                }

                for( int i = 0; i < count; i++ )
                {
                    rect = EditorGUILayout.BeginHorizontal( GUILayout.Height( lh ) );
                    rect.RequestTop( lh );
                    GUILayout.Space( lh );
                    GUI.Label( rect.VerticalSlice( 0, slices ), _namesCache[i] );
                    for( int j = minE; j <= maxE; j++ )
                    {
                        double val = 0;
                        for (int k = rMin; k <= rMax; k++)
                        {
                            var chance = M[i, j, k];
                            if (_rollChancesCache != null && k - rMin < _rollChancesCache.Length) chance *= _rollChancesCache[k - rMin];
                            val += chance;
                        }
                        val /= sumWeight;
                        val = System.Math.Max( val, 0 );

                        var r = rect.VerticalSlice( ( j - minE ) + 1, slices );
                        GUIProgressBar.Draw( r, (float)val );
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
        }

        public float GetPermutationsHeight()
        {
            return EditorGUIUtility.singleLineHeight * _permutations.Count;
        }

        public void DrawPermutations( Rect rect )
        {
            var maxLabelSize = 24f;
            GUIStyle labelStyle = GUI.skin.label;
            foreach(var permutation in _permutations ) 
            {
                var size = labelStyle.CalcSize(new GUIContent(permutation.name)).x;
                if( maxLabelSize > size ) continue;
                maxLabelSize = size;
            }
            var labelHeight = EditorGUIUtility.singleLineHeight;

            if( maxLabelSize > rect.width - 32 ) maxLabelSize = rect.width - 32;

            foreach(var permutation in _permutations )
            {
                var line = rect.GetLineTop( labelHeight );
                GUI.Label( line.RequestLeft( maxLabelSize ), permutation.name, labelStyle );
                GUIProgressBar.Draw( line.CutLeft( maxLabelSize ), permutation.percentage );
            }
        }

        public void DrawPermutations()
        {
            EditorGUILayout.BeginVertical();
            var maxSize = 24f;
            GUIStyle labelStyle = GUI.skin.label;
            foreach(var permutation in _permutations ) 
            {
                var size = labelStyle.CalcSize(new GUIContent(permutation.name)).x;
                if( maxSize > size ) continue;
                maxSize = size;
            }
            var labelWidth = GUILayout.Width(maxSize);
            var labelHeight = GUILayout.Height( EditorGUIUtility.singleLineHeight );

            foreach(var permutation in _permutations )
            {
                EditorGUILayout.BeginHorizontal( labelHeight );
                GUILayout.Label( permutation.name, labelStyle, labelWidth, labelHeight );
                GUIProgressBar.DrawLayout( permutation.percentage, GUILayout.MinWidth( 32 ) );
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        private int PermutationSorting((string, float) x, (string, float) y) => y.Item2.CompareTo( x.Item2 );

        void Permutations()
        {
            var rollsCount = rMax + 1 - rMin;
            var defaultChance = 1f / rollsCount;
            var rChanLen = _rollChancesCache?.Length ?? 0;
            if( rChanLen > rollsCount ) rChanLen = rollsCount;
            var biased = rChanLen > 0;

            if( _countCache == null || _countCache.Length < count ) _countCache = new int[count];

            var baseMins = 0;
            for (int i = 0; i < _minCache.Length; i++)
            {
                baseMins += _minCache[i];
                _countCache[i] = _minCache[i];
            }

            _permutationChances.Clear();

            var rwSum = 0f;
            for (int i = 0; i < rChanLen; i++) rwSum += _rollChancesCache[i];

            Score.StartAccumulator();
            ElementsCount.StartAccumulator();

            combinations.Clear();
            combinationsChances.Clear();

            for( int i = 0; i < rollsCount; i++ )
            {
                var rolls = i + rMin;
                var chance = defaultChance;
                if( biased ) chance = _rollChancesCache[ Mathf.Min( i, rChanLen - 1 )] / rwSum;
                Permute(baseMins, rolls, chance );
            }

            _permutations.Clear();
            foreach( var e in _permutationChances ) _permutations.Add( ( e.Key, e.Value ) );
            _permutationChances.Clear();
        }

        int CalculateScore()
        {
            int score = 0;
            for( int i = 0; i < _countCache.Length; i++ )
            {
                score += _countCache[i];
                if( _scorer != null ) score *= _scorer(_elements[i]);
            }
            return score;
        }
        
        void Permute( int it, int maxPermute, float currChance )
        {
            if( it >= maxPermute )
            {
                var name = NameCombination(_countCache, _namesCache, it);
                _permutationChances.TryGetValue(name, out var chance);

                var realChance = chance + currChance;
                _permutationChances[name] = realChance;
                
                Score.RegisterValue( CalculateScore(), currChance );
                ElementsCount.RegisterValue( maxPermute, currChance );

                var combArray = new int[_countCache.Length];
                for( int i = 0; i < _countCache.Length; i++ ) combArray[i] = _countCache[i];
                combinations.Add( combArray );
                combinationsChances.Add( currChance );

                return;
            }
            var sumWeights = 0f;

            var count = _maxCache.Length;
            for( int k = 0; k < count; k++ )
            {
                if (_maxCache[k] <= _countCache[k]) continue;
                var eChan = _chancesCache[k];
                sumWeights += eChan;
            }
            
            for( int k = 0; k < count; k++ )
            {
                if (_maxCache[k] <= _countCache[k]) continue;
                var eChan = _chancesCache[k];
                _countCache[k]++;
                Permute( it + 1, maxPermute, currChance * ( eChan / sumWeights ) );
                _countCache[k]--;
            }
        }

        private string NameCombination( int[] _countCache, string[] names, int maxRolls = int.MaxValue)
        {
            var sb = StringBuilderPool.RequestEmpty();

            var first = true;
            for (int i = 0; i < _countCache.Length; i++)
            {
                var count = Mathf.Min( _countCache[i], maxRolls );
                if (count > 0)
                {
                    if( !first ) sb.Append( ", " );
                    sb.Append($"{count} {names[i]}");
                    first = false;
                }
                maxRolls -= count;
            }

            if( first ) sb.Append( "NOTHING" );

            return sb.ReturnToPoolAndCast();
        }
    }
