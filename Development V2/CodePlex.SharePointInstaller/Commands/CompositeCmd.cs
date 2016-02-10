using System;
using System.Collections.Generic;

namespace CodePlex.SharePointInstaller.Commands
{
    public delegate void CommandAction();

    public class CompositeCmd : Cmd
    {
        private IList<Cmd> commands = new List<Cmd>();

        private Stack<Cmd> executed = new Stack<Cmd>();

        private CommandAction beforeExecute;

        private CommandAction afterExecute;

        private String description;

        public CompositeCmd(String description, CommandAction beforeExecute, CommandAction afterExecute)
        {
            this.description = description;
            this.beforeExecute = beforeExecute;
            this.afterExecute = afterExecute;
        }

        public override bool Execute()
        {
            if (beforeExecute != null && commands.Count > 0)
                beforeExecute();

            foreach(var cmd in commands)
            {
                if (cmd.Execute())
                    executed.Push(cmd);
                else
                {
                    executed.Push(cmd);
                    return false;                    
                }
            }

            if (afterExecute != null && commands.Count > 0)
                afterExecute();

            return true;
        }

        public override bool Rollback()
        {
            while(executed.Count > 0)
            {
                var cmd = executed.Pop();
                if (!cmd.Rollback())
                    return false;
            }
            return true;
        }

        public void Add(Cmd cmd)
        {
            commands.Add(cmd);
        }

        public override String Description
        {
            get
            {
                return description;
            }
        }
    }
}
