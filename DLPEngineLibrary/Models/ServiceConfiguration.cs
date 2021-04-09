
namespace DLPEngineLibrary.Models
{
    internal class ServiceConfiguration
    {
        public string Email { get; set; } = default(string);

        public bool IsEmpty
        {
            get
            {
                System.Type type = typeof(ServiceConfiguration);
                foreach (System.Reflection.PropertyInfo property in type.GetProperties())
                    if (type.GetProperty(property.Name).GetValue(this, null) != null)
                        return false;

                return true;
            }
        }
    }
}
