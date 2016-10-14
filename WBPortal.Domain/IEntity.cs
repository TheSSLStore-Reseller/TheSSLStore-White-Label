using System.ComponentModel.DataAnnotations;

namespace WBSSLStore.Domain
{
    /// <summary>
    /// Base Interface that every class must implement to store data
    /// </summary>
    public interface IEntity
    {
        int ID { get; }
    }
}
