﻿/*
 * Created by SharpDevelop.
 * User: Alexander Petrovskiy
 * Date: 7/17/2014
 * Time: 7:07 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

namespace Tmx.Client.Helpers.Commands
{
	using System;
	using Tmx;
	using Tmx.Client.Helpers;
	using Tmx.Commands;
	
	/// <summary>
	/// Description of RegisterSystemUnderTestCommand.
	/// </summary>
    class RegisterSystemUnderTestCommand : TmxCommand
    {
        internal RegisterSystemUnderTestCommand(CommonCmdletBase cmdlet) : base (cmdlet)
        {
        }
        
        internal override void Execute()
        {
            var cmdlet = (RegisterTmxSystemUnderTestCommand)Cmdlet;
            ClientSettings.ServerUrl = cmdlet.ServerUrl;
            // Tmx.Client.Helpers.ClientSettings.ServerUrl = cmdlet.ServerUrl;
        }
    }
}