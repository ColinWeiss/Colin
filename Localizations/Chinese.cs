using System;
using System.Collections.Generic;
using System.Text;

namespace Colin.Core.Localizations
{
    public class Chinese : GameLanguagePackage
    {
        public override void SetDefaultTexts( )
        {
            Common.Add( "ON" , "开" );
            Common.Add( "OFF", "关" );
            base.SetDefaultTexts( );
        }
    }
}