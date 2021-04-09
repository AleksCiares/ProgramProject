
namespace DLPEngineLibrary.Models
{
    internal class ServiceConfig
    {
        public string Email { get; set; }

        public bool IsEmpty()
        {
            System.Type type = typeof(ServiceConfig);
            foreach (System.Reflection.PropertyInfo property in type.GetProperties())
                if (type.GetProperty(property.Name).GetValue(this, null) != null)
                    return false;

            return true;
        }
    }
}
