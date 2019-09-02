using System.Linq;

namespace Clink.Cdn.Invalidate
{
    class CommandArgsMapper
    {
        private CommandArgsModel _commandArgsModel;

        public CommandArgsMapper()
        {
            _commandArgsModel = new CommandArgsModel();
        }


        public CommandArgsModel Map(string[] args)
        {
            MapCommandlineArgs(args);
            return _commandArgsModel;
        }

        private void MapCommandlineArgs(string[] args)
        {
            for (var i = 0; i < args.Length; i++)
            {
                GetCommandValues(args[i], args[++i]);
            }
        }

        private void GetCommandValues(string key, string value)
        {
            switch (key)
            {
                case "--nid":
                    _commandArgsModel.NetworkId = value;
                    break;

                case "--paths":
                    if (!string.IsNullOrEmpty(value))
                        _commandArgsModel.Paths = value.Split(",").Select(path => path.Trim()).ToArray();
                    break;
                case "--email":
                    _commandArgsModel.Email = value;
                    break;
                case "--useProxy":
                    _commandArgsModel.UseProxy = bool.Parse(value);
                    break;
            }
        }
    }
}
