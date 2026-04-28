using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Foundation.Sequence
{
    public class SequenceNode
    {
        private Action<UniTaskCompletionSource> _action;

        private string _tag;
        private UniTaskCompletionSource _taskSource;
        public SequenceNode(Action<UniTaskCompletionSource> action, string inNodeTag)
        {
            _action = action;
            _tag = inNodeTag;
            _taskSource = new UniTaskCompletionSource();
        }

        public void ResetTask()
        {
            _taskSource = null;
            _taskSource = new UniTaskCompletionSource();
        }

        public bool FinishTask()
        {
            if (_taskSource != null && _taskSource.UnsafeGetStatus() != UniTaskStatus.Succeeded)
            {
                _taskSource.TrySetResult();
                return true;
            }

            return false;
        }

        public async UniTask Execute(CancellationTokenSource cancellationTokenSource)
        {
            if (cancellationTokenSource.IsCancellationRequested)
                return;
            _action.Invoke(_taskSource);
            Debug.Log("entrance task   "+_tag);
            await _taskSource.Task;
        }

        public bool Is(string nodeTag)
        {
            if (string.IsNullOrEmpty(nodeTag))
                return true;
            return _tag == nodeTag;
        }
    }
    public class FSequence
    {
        
        private List<SequenceNode> nodeList;

        private bool _stopRequested = false;
        private SequenceNode currentNode = null; 

        public FSequence()
        {
        }

        public virtual void InitList(List<SequenceNode> inNodeList)
        {
            if (nodeList != null && nodeList.Count > 0)
            {
                nodeList.Clear();
            }
            nodeList = inNodeList;
            currentNode = null;
            _stopRequested = false;
        }

        public async void Start( CancellationTokenSource cancellationTokenSource)
        {
            for (int i = 0; i < nodeList.Count; i++)
            { 
                if (cancellationTokenSource.IsCancellationRequested || _stopRequested)
                {
                    currentNode = null;
                    return;
                }
                
                currentNode = nodeList[i];
                
                await nodeList[i].Execute(cancellationTokenSource);
            }
            
            Debug.Log("Sequence Action is Finish!!!!!!!!!!!!!");
        }
        public void Stop()
        {
            _stopRequested = true; 
            currentNode?.FinishTask();
        }

        public bool FinishCurrentTask(string tagName)
        {
            if (currentNode != null && currentNode.Is(tagName))
            {
                return currentNode.FinishTask();
            }
            
            return false;
        }

        public void FinishCurrentTaskToLobby()
        {
            if (currentNode != null)
            {
                currentNode.FinishTask();
            }
        }
        public void ResetTask()
        {
            for (int i = 0; i < nodeList.Count; i++)
            {
                nodeList[i].ResetTask();
            }
        }
    }
}