namespace EventWriter
{
    public class TimeSlot
    {
        private readonly List<Registration> _currentRegistrations;
        public int MaxCapacity { get; private set; }
        public DateTime Time { get; private set; }
        public int CurrentRegistrationCount => _currentRegistrations.Count;

        public TimeSlot(DateTime time, int maxCapacity)
        {
            if (maxCapacity < 0)
            {
                throw new ArgumentException("Максимальная вместимость не может быть отрицательной", nameof(maxCapacity));
            }
            Time = time;
            MaxCapacity = maxCapacity;
            _currentRegistrations = new List<Registration>();
        }

        public bool CanRegister(Registration registration)
        {
            if (CurrentRegistrationCount == MaxCapacity || HasUser(registration.UserId))
            {
                return false;
            }
            return true;
        }

        public bool AddRegistration(Registration registration)
        {
            if (CanRegister(registration))
            {
                _currentRegistrations.Add(registration);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void RemoveRegistration(Registration registration)
        {
            if (HasUser(registration.UserId))
            {
                _currentRegistrations.Remove(registration);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public void EditTime(DateTime time)
        {
            if (time == default || Time == time)
            {
                return;
            }

            Time = time;
        }

        public void EditCapacity(int capacity)
        {
            if (capacity == MaxCapacity)
            {
                return;
            }

            if (capacity < 0)
            {
                throw new ArgumentException("Максимальная вместимость не может быть отрицательной", nameof(capacity));
            }

            if (capacity < MaxCapacity && CurrentRegistrationCount > capacity)
            {
                throw new ArgumentException();
            }

            MaxCapacity = capacity;
        }
        private bool HasUser(long userId)
        {
            return _currentRegistrations.FirstOrDefault(r => r.UserId == userId) != null;
        }
    }
}