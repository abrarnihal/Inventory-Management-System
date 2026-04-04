using coderush.Data;
using System;
using System.Linq;
using System.Threading;

namespace coderush.Services
{
    public class NumberSequence(ApplicationDbContext context) : INumberSequence
    {
        private readonly ApplicationDbContext _context = context;

        public string GetNumberSequence(string module)
        {
            string result = "";
            int counter = 0;

            Models.NumberSequence numberSequence = _context.NumberSequence
                .Where(x => x.Module.Equals(module))
                .FirstOrDefault();

            if (numberSequence == null)
            {
                numberSequence = new Models.NumberSequence
                {
                    Module = module
                };
                Interlocked.Increment(ref counter);
                numberSequence.LastNumber = counter;
                numberSequence.NumberSequenceName = module;
                numberSequence.Prefix = module;

                _context.Add(numberSequence);
                _context.SaveChanges();
            }
            else
            {
                counter = numberSequence.LastNumber;

                Interlocked.Increment(ref counter);
                numberSequence.LastNumber = counter;

                _context.Update(numberSequence);
                _context.SaveChanges();
            }

            result = numberSequence.Prefix + counter.ToString().PadLeft(5, '0');
            return result;
        }
    }
}
