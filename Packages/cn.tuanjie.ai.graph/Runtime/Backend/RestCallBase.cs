using System;
using System.Collections.Generic;

namespace UnityEngine.AIGraph.Backend
{
    internal interface IQuarkEndpoint
    {
        public enum EMethod
        {
            GET,
            POST,
            DELETE,
            PATCH
        }

        public string server { get; }
        public string endPoint { get; }
        public EMethod method { get; }
    }
    internal abstract class QuarkRestCall : IDisposable
    {
        public enum EState
        {
            None,
            WaitingForDependency,
            InProgress,
            Completed,
            Error,
            Retrying,
            Forbidden
        }
        List<QuarkRestCall> m_Dependencies = new List<QuarkRestCall>();
        public IReadOnlyList<QuarkRestCall> dependencies => m_Dependencies;

        public event Action<QuarkRestCall> onCompleted = _ => { };


        public void SendRequest()
        {
            // we are already done
            if (isCompleted)
                onCompleted?.Invoke(this);
            if (m_Dependencies.Count > 0)
            {
                restCallState = EState.WaitingForDependency;
                for (int i = 0; i < m_Dependencies.Count; ++i)
                {
                    if (!m_Dependencies[i].isCompleted)
                    {
                        m_Dependencies[i].onCompleted += OnDependencyComplete;
                        m_Dependencies[i].SendRequest();
                    }
                }
            }
            else if (restCallState != EState.InProgress)
            {
                restCallState = EState.InProgress;
                MakeServerRequest();
            }

        }

        void OnDependencyComplete(QuarkRestCall dependency)
        {
            dependency.onCompleted -= OnDependencyComplete;
            int i = 0;
            for (; i < m_Dependencies.Count; ++i)
            {
                if (!m_Dependencies[i].isCompleted)
                    break;
            }

            if ((restCallState == EState.None || restCallState == EState.WaitingForDependency) && i >= m_Dependencies.Count)
            {
                restCallState = EState.InProgress;
                MakeServerRequest();
            }

        }

        protected abstract void MakeServerRequest();

        public EState restCallState { get; set; }
        public bool isCompleted => restCallState == EState.Completed || restCallState == EState.Error;
        public bool isError => restCallState == EState.Error;
        public QuarkRestCall DependOn(QuarkRestCall call)
        {
            if (restCallState != EState.None)
            {
                //since we have started, we need to start the dependency as well
                call.onCompleted += OnDependencyComplete;
                call.SendRequest();
            }
            m_Dependencies.Add(call);
            return this;
        }

        protected void SignalRequestCompleted(EState state)
        {
            restCallState = state;
            onCompleted?.Invoke(this);
        }

        public virtual void Dispose()
        {
            for (int i = 0; i < m_Dependencies.Count; ++i)
            {
                m_Dependencies[i]?.Dispose();
            }

            m_Dependencies?.Clear();
            onCompleted = null;
        }
    }

    internal class QuarkRestCallJob : QuarkRestCall
    {
        List<QuarkRestCall> m_RestCalls = new List<QuarkRestCall>();
        protected override void MakeServerRequest()
        {
            if (m_RestCalls.Count == 0)
                SignalRequestCompleted(EState.Completed);
            else
            {
                for (int i = 0; i < m_RestCalls.Count; ++i)
                {
                    m_RestCalls[i].onCompleted += OnRestCallCompleted;
                    m_RestCalls[i].SendRequest();
                }
            }
        }

        public QuarkRestCallJob AddCall(QuarkRestCall call)
        {
            //if (isCompleted)
            //    throw new Exception("Request already in completed state");
            m_RestCalls.Add(call);
            // we are already in progress, start the call
            if (restCallState != EState.None && !call.isCompleted)
            {
                call.onCompleted += OnRestCallCompleted;
                call.SendRequest();
            }

            return this;
        }

        void OnRestCallCompleted(QuarkRestCall call)
        {
            //check if all calls are completed
            int i = 0;
            bool hasError = false;
            for (; i < m_RestCalls.Count; ++i)
            {
                if (!m_RestCalls[i].isCompleted)
                {
                    if (m_RestCalls[i].isError)
                        hasError = true;
                    break;
                }

            }

            if (i >= m_RestCalls.Count)
            {
                restCallState = hasError ? EState.Error : EState.Completed;
                SignalRequestCompleted(restCallState);
            }
        }
    }
}