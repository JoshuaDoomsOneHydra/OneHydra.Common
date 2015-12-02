using System;

namespace OneHydra.Common.Utilities.Test.Random
{
    public static class Numbers
    {
        #region Fields

        private static readonly System.Random Random = new System.Random();


        #endregion Fields

        #region Methods

        public static decimal? NullableDecimal(bool includeNullValues = true)
        {
            var returnValue = new Decimal?();
            var getNullValue = Random.Next(5) == 1;
            if (!includeNullValues || !getNullValue)
            {
                var scale = (byte) Random.Next(29);
                var sign = Random.Next(2) == 1;
                returnValue = new decimal(Random.Next(), Random.Next(), Random.Next(), sign, scale);
            }
            return returnValue;
        }


        public static int? NullableInt(bool includeNullValues = true)
        {
            var returnValue = new int?();
            var getNullValue = Random.Next(5) == 1;
            if (!includeNullValues || !getNullValue)
            {
                returnValue = Random.Next();

            }
            return returnValue;
        }

        #endregion Methods

    }
}
