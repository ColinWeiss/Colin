namespace Colin.Common.Code.UI
{
    internal class ContainerPage : Container
    {
        public override sealed bool GetInterviewState( )
        {
            return false;
        }
        protected override sealed void SetLayerout( ref ContainerElement containerElement )
        {
            containerElement.SetLayerout(0,0,EngineInfo.GameViewWidth,EngineInfo.GameViewHeight);
            base.SetLayerout(ref containerElement);
        }
    }
}