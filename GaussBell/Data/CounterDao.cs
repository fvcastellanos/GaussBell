namespace GaussBell.Data
{
    public class CounterDao
    {
        private static int value;

        public CounterDao()
        {
            value = 0;
        }

        public int GetCounterValue()
        {
            return value;
        }

        public void SetCounterValue(int value)
        {
            CounterDao.value = value;
        }
    }
}