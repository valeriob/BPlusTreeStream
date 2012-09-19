using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Benchmarks
{
    public abstract class Benchmark
    {

        public abstract void Run(int count, int batch);

        public virtual void Prepare(int count, int batch) { }        

    }

    public class Result
    {
        public string Name { get; set; }

        public DateTime Start { get; set; }
        public DateTime Stop { get; set; }
        public int Count { get; set; }
        public int? Batch { get; set; }

        public override string ToString()
        {
            var delta = Stop - Start;
            var speed = delta.TotalSeconds == 0 ? float.PositiveInfinity: Count / delta.TotalSeconds;
            var batch = Batch.HasValue ? "( " + Batch + " )" : "";

            return string.Format("{0} -   {1} {2} tx in {3:0.000}. {4:0.000} tx/s", Name, Count, batch, delta.TotalSeconds, speed);
        }
    }
}
