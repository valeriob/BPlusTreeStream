using Raven.Client.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Benchmarks
{
    public class Ravendb : Benchmark
    {
        DocumentStore _documentStore;
        public Ravendb()
        {
            _documentStore = new DocumentStore { ConnectionStringName = "Benchmark" };
            _documentStore.Initialize();
        }

        public override void Run(int count, int batch)
        {
            for (int i = 0; i < count; i += batch) 
            {
                using (var session = _documentStore.OpenSession())
                {
                    for (int j = i; j < i+batch; j++)
                    {
                        session.Store(new My_Test_Entity { Id= Guid.NewGuid(), Body = "test "+j, Timestamp =DateTime.Now });
                    }
                    session.SaveChanges();
                }
            }
        }
    }

    public class My_Test_Entity 
    {
        public Guid Id { get; set; }
        public string Body { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
