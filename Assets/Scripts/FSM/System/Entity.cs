using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Percent111.ProjectNS.FSM.System
{

    public class Entity : MonoBehaviour
    {
        // Entity의 고유 ID
        private static int nextID;
        private int id;
        public int ID
        {
            set
            {
                id = value;
                nextID++;
            }
            get => id;
        }

        // 초기화
        public virtual void Init()
        {
            ID = nextID;
        }
    }
}
