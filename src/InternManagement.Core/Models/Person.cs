namespace InternManagement.Core.Models
{
    public abstract class Person
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        public abstract string RoleName { get; }

        public virtual string GetDisplayInfo()
        {
            return RoleName + ": " + FullName + " (" + Email + ")";
        }

        public override string ToString()
        {
            return FullName;
        }
    }
}
