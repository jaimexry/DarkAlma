using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JB
{
    public class State
    {
        private bool forceExit;
        private List<StateAction> fixedUpdateActions;
        private List<StateAction> updateActions;
        private List<StateAction> lateUpdateActions;

        public delegate void OnEnter();
        public OnEnter onEnter;

        public State(List<StateAction> fixedUpdateActions, List<StateAction> updateActions,
            List<StateAction> lateUpdateActions)
        {
            this.fixedUpdateActions = fixedUpdateActions;
            this.updateActions = updateActions;
            this.lateUpdateActions = lateUpdateActions;
        }

        public void FixedTick()
        {
            ExecuteListOfActions(fixedUpdateActions);
        }
        
        public void Tick()
        {
            ExecuteListOfActions(updateActions);
        }

        public void LateTick()
        {
            ExecuteListOfActions(lateUpdateActions);
            forceExit = false;
        }

        private void ExecuteListOfActions(List<StateAction> l)
        {
            for (int i = 0; i < l.Count; i++)
            {
                if (forceExit)
                {
                    return;
                }

                forceExit = l[i].Execute();
            }
        }
    }
}

