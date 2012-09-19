using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Benchmarks
{
    public class SqlServer : Benchmark, IDisposable
    {
        SqlConnection con;
        public SqlServer()
        { 
            con = new SqlConnection(@"Data Source=(localdb)\Projects;Initial Catalog=Test;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False");
            con.Open();

            using(var cmd = con.CreateCommand())
            {
                cmd.CommandText = "TRUNCATE TABLE Test.dbo.Table_Insert";
                cmd.ExecuteNonQuery();
            }
        }
        public override void Run(int count, int batch)
        {
            for (int i = 0; i < count; i++)
            {
                var trans = con.BeginTransaction();
                for (int j = i; j < batch + i; j+= batch)
                {
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.Transaction = trans;
                        cmd.CommandText = @"INSERT INTO dbo.TABLE_Insert VALUES(@id, @value)";
                        var par = cmd.CreateParameter();
                        par.Value = j;
                        par.ParameterName = "id";
                        cmd.Parameters.Add(par);

                        par = cmd.CreateParameter();
                        par.Value = "value " + j;
                        par.ParameterName = "value";
                        cmd.Parameters.Add(par);

                        cmd.ExecuteNonQuery();
                    }
                    trans.Commit();
                }
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
