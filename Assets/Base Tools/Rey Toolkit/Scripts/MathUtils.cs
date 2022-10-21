namespace ReyToolkit
{
    public static class MathUtils
    {
        // A replacement for the mod operator '%' that allows wrapping to continue without discontinuities across 0 when 'negWrap' is set to true.
        public static int Mod(int a, int b, bool negWrap = true)
        {
            int c = a % b;

            if (negWrap && a < 0)
                c += b;

            return c;
        }

        // Convert an array of floats to an array of percentages, where each is the percentage of the original float out of all of them summed together.
        // If 'stackPercentages' is set to true, then all previous percentages in the array are added to each percentage.
        // This way they represent percentages of the way between 0 and 1.
        public static float[] ConvertToPercents(float[] flatValues, bool stackPercentages = false)
        {
            float totalValue = 0.0f;
            for (int i = 0; i < flatValues.Length; i++)
                totalValue += flatValues[i];

            float[] percentValues = new float[flatValues.Length];
            for (int i = 0; i < flatValues.Length; i++)
                percentValues[i] = flatValues[i] / totalValue;

            if (stackPercentages)
            {
                for (int i = 1; i < percentValues.Length; i++)
                    percentValues[i] += percentValues[i - 1];
            }

            return percentValues;
        }
    }
}
