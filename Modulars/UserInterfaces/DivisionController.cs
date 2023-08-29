using Colin.Core.Inputs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colin.Core.Modulars.UserInterfaces
{
    public class DivisionController
    {
        private Division _division;
        public Division Division => _division;
        public DivisionController( Division division ) => _division = division;
        public virtual void OnInit( ) { }
        public virtual void Layout( ref LayoutStyle layout ) { }
        public virtual void Interact( ref InteractStyle interact )
        {
            interact.InteractionLast = interact.Interaction;
            if( _division.ContainsPoint( MouseResponder.State.Position ) && _division.Interact.IsInteractive )
                interact.Interaction = true;
            else
                interact.Interaction = false;
        }
        public virtual void Design( ref DesignStyle design ) { }
    }
}