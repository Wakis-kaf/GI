using System.Data;

namespace UnitFramework.Runtime
{
    public interface IExcelDataReceiver
    {
        void InitWith(DataSet dataSet);
    }
}