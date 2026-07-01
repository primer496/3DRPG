using System;
using System.Collections;

namespace UnityEngine.AIGraph
{
    [Serializable]
    public class ProcessorException : Exception
    {
        // 默认构造函数
        public ProcessorException() : base() { }

        // 带消息的构造函数
        public ProcessorException(string message) : base(message) { }

        // 带消息和内层异常的构造函数
        public ProcessorException(string message, Exception innerException) 
            : base(message, innerException) { }

        // 可选的：添加上下文信息
        public virtual string Context { get; set; } = "Coroutine Processor";
    }
    /// <summary>
    /// General interface for invoking asynchronous routine with success flag and exception handling.
    /// </summary>
    public class CoroutineProcessor
    {
        private bool success;

        private Exception e;

        /// <summary>
        /// True if the node is successfully processed, false otherwise
        /// </summary>
        public virtual bool Success
        {
            get => success;
            protected set => success = value;
        }

        /// <summary>
        /// Store exception thrown during node process for further handling
        /// </summary>
        public virtual Exception Ex
        {
            get => e;
            protected set => e = value;
        }

        /// <summary>
        /// Reset status
        /// </summary>
        public virtual void Reset()
        {
            success = true;
            e = null;
        }

        public virtual IEnumerator ProcessAsync(IEnumerator routine)
        {
            Reset();
            while (true)
            {
                try
                {
                    if (!routine.MoveNext())
                        yield break;
                }
                catch (Exception e)
                {
                    Success = false;
                    Ex = e;
                    yield break;
                }

                yield return routine.Current;
            }
        }

        public virtual string HandleException()
        {
            if (Ex != null)
            {
                // Debug.LogException(Ex);
                throw new ProcessorException("", Ex);
            }
            return Ex?.Message ?? "";
        }
    }

    /// <summary>
    /// General interface for invoking asynchronous routine with return value.
    /// </summary>
    public class CoroutineProcessor<T> : CoroutineProcessor
    {
        protected T result;

        public virtual T Result
        {
            get
            {
                if(!Success)
                    HandleException();

                return result;
            }
            protected set => result = value;
        }

        public override void Reset()
        {
            base.Reset();
            result = default(T);
        }

        public override IEnumerator ProcessAsync(IEnumerator routine)
        {
            Reset();
            while (true)
            {
                try
                {
                    if (!routine.MoveNext())
                        yield break;
                }
                catch (Exception e)
                {
                    Success = false;
                    Ex = e;
                    yield break;
                }

                object current = routine.Current;
                if (current != null && typeof(T).IsAssignableFrom(current.GetType()))
                    Result = (T)current;

                yield return current;
            }
        }
    }
}
