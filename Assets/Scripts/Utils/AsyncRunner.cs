using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FinalRPG.Utils
{
    /// <summary>
    /// 轻量异步任务编排器 — Task + 轮询模式，不阻塞主线程。
    /// 参考 HSM SequentialPhase 的设计，抽取为通用工具。
    ///
    /// 用法：
    /// <code>
    ///   _runner.RunSequential(new Func&lt;CancellationToken, Task&gt;[] {
    ///       ct => Task.Run(() => File.ReadAllText(path), ct),   // Step 1: 线程池
    ///       ct => { RestoreFromJson(json); return Task.CompletedTask; }  // Step 2: 主线程回调
    ///   }, onComplete: () => RPGLog.Debug("OK"));
    ///
    ///   void Update() => _runner.Tick();
    /// </code>
    ///
    /// 宿主在 Update 中调用 Tick() 驱动，每帧检查当前 Task.IsCompleted。
    /// 前一步完成后自动启动下一步，全部完成时触发 onComplete 回调。
    /// </summary>
    public class AsyncRunner
    {
        readonly List<Func<CancellationToken, Task>> _steps = new List<Func<CancellationToken, Task>>();
        CancellationTokenSource _cts;
        Task _currentTask;
        Action _onComplete;
        int _index;

        /// <summary>是否有任务正在执行中。</summary>
        public bool IsRunning => _currentTask != null && !_currentTask.IsCompleted;

        /// <summary>启动单任务，完成时触发 onComplete。</summary>
        public void Run(Func<CancellationToken, Task> factory, Action onComplete = null)
        {
            Cancel();
            _onComplete = onComplete;
            _cts = new CancellationTokenSource();
            _index = -1;
            _steps.Clear();
            _steps.Add(factory);
            Next();
        }

        /// <summary>
        /// 启动串行多步任务，前一步完成后自动启动下一步。
        /// 全部完成时触发 onComplete。
        /// </summary>
        public void RunSequential(IReadOnlyList<Func<CancellationToken, Task>> steps, Action onComplete = null)
        {
            Cancel();
            _onComplete = onComplete;
            _cts = new CancellationTokenSource();
            _index = -1;
            _steps.Clear();
            _steps.AddRange(steps);
            Next();
        }

        /// <summary>每帧调用，检查当前任务是否完成并推进序列。</summary>
        public void Tick()
        {
            if (_currentTask == null) return;
            if (!_currentTask.IsCompleted) return;

            if (_currentTask.IsFaulted)
            {
                var ex = _currentTask.Exception?.InnerException ?? _currentTask.Exception;
                RPGLog.Error("AsyncRunner", $"Task faulted: {ex?.Message}");
                Cleanup();
                return;
            }

            Next();
        }

        /// <summary>取消当前任务链，清空状态。</summary>
        public void Cancel()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
            Cleanup();
        }

        void Next()
        {
            _index++;
            if (_index >= _steps.Count)
            {
                var cb = _onComplete;
                Cleanup();
                cb?.Invoke();
                return;
            }

            try
            {
                _currentTask = _steps[_index](_cts.Token);
            }
            catch (Exception ex)
            {
                RPGLog.Error("AsyncRunner", $"Step {_index} threw: {ex.Message}");
                Cleanup();
            }
        }

        void Cleanup()
        {
            _currentTask = null;
            _steps.Clear();
            _index = -1;
            _onComplete = null;
        }
    }
}
