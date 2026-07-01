using System.Collections;
using GraphProcessor;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;

namespace UnityEngine.AIGraph
{
    [System.Serializable, NodeMenuItem("Tools/Animator Creator"), UseProcessAsync]
    public class AnimatorCreatorNode : SDNode
    {
        [Input(name = "AnimationClip"), SerializeField, HideInInspector] private AnimationClip m_Clip;
        [Input(name = "GameObject"), SerializeField, HideInInspector] private GameObject m_GO;
        [SerializeField, HideInInspector] public AnimatorController m_Controller;

        public override bool isRenamable => false;
        public override bool needTrigger => true;
        public override IEnumerator ProcessAsync()
        {
            if (m_GO == null || m_Clip == null)
                yield break;


            m_Controller = AnimatorController.CreateAnimatorControllerAtPath(AssetDatabase.GenerateUniqueAssetPath($"Assets/{m_GO.name}.controller"));

            var rootStateMachine = m_Controller.layers[0].stateMachine;
            foreach (var state in rootStateMachine.states)
            {
                rootStateMachine.RemoveState(state.state);
            }

            var animationState = rootStateMachine.AddState(m_Clip.name);
            animationState.motion = m_Clip;

            rootStateMachine.defaultState = animationState;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var animator = m_GO.GetComponent<Animator>();
            if (animator == null)
            {
                animator = m_GO.AddComponent<Animator>();
            }

            animator.runtimeAnimatorController = m_Controller;

            yield return null;
        }

        public IEnumerator Generate() => ProcessAsync();
    }
}
#endif