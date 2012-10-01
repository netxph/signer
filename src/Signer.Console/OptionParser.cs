using NDesk.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Signer.Console
{
    public class OptionParser
    {

        protected virtual OptionSet CreateOptions(SignerSetting setting)
        { 
            var optionSet = new OptionSet
            {
                { "key=", v => setting.KeyFile = v }
            };

            return optionSet;
        }

        public SignerSetting Parse(string[] args)
        {
            SignerSetting setting = new SignerSetting();

            var optionSet = CreateOptions(setting);
            var extra = optionSet.Parse(args);

            setting.Path = string.Join(" ", extra.ToArray());

            return setting;
        }

    }
}
