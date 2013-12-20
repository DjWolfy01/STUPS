﻿/*
 * Created by SharpDevelop.
 * User: Alexander Petrovskiy
 * Date: 12/4/2013
 * Time: 10:33 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

namespace UIAutomation
{
    extern alias UIANET;
    using System;
    using System.Windows.Automation;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using PSTestLib;
    
    /// <summary>
    /// Description of ExtensionMethodsElement.
    /// </summary>
    public static class ExtensionMethodsElement
    {
        public static IUiElement GetParent(this IUiElement element)
        {
            IUiElement result = null;
            
            TreeWalker walker =
                new TreeWalker(
                    System.Windows.Automation.Condition.TrueCondition);
            
            try {
                result = AutomationFactory.GetUiElement(walker.GetParent(element.GetSourceElement()));
            }
            catch {}
            
            return result;
        }
        
        #region get an ancestor with a handle
        /// <summary>
        ///  /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        internal static IUiElement GetAncestorWithHandle(this IUiElement element)
        {
            TreeWalker walker =
                new TreeWalker(
                    System.Windows.Automation.Condition.TrueCondition);
            
            try {
                
                IUiElement testparent = AutomationFactory.GetUiElement(walker.GetParent(element.GetSourceElement()));
                while (testparent != null &&
                       testparent.Current.NativeWindowHandle == 0) {
                    testparent =
                        AutomationFactory.GetUiElement(walker.GetParent(testparent.GetSourceElement()));
                    if (testparent != null &&
                        (int)testparent.Current.ProcessId > 0 &&
                        testparent.Current.NativeWindowHandle != 0) {
                        
                        return testparent;
                    }
                }
                return testparent.Current.NativeWindowHandle != 0 ? testparent : null;
                
            } catch {
                return null;
            }
        }
        #endregion get an ancestor with a handle
        
        #region get the parent or an ancestor
        /// <summary>
        ///  /// </summary>
        /// <param name="element"></param>
        /// <param name="scope"></param>
        /// <returns></returns>
        internal static IUiElement[] GetParentOrAncestor(this IUiElement element, TreeScope scope)
        {
            TreeWalker walker =
                new TreeWalker(
                    System.Windows.Automation.Condition.TrueCondition);
            
            List<IUiElement> ancestors =
                new List<IUiElement>();
            
            try {
                
                IUiElement testParent = AutomationFactory.GetUiElement(walker.GetParent(element.GetSourceElement()));
                    
                if (scope == TreeScope.Parent || scope == TreeScope.Ancestors) {
                    
                    if (testParent != UiElement.RootElement) {
                        ancestors.Add(testParent);
                    }
                    
                    if (testParent == UiElement.RootElement ||
                        scope == TreeScope.Parent) {
                        return ancestors.ToArray();
                    }
                }
                while (testParent != null &&
                       (int)testParent.Current.ProcessId > 0 &&
                       testParent != UiElement.RootElement) {
                    
                    testParent =
                        AutomationFactory.GetUiElement(walker.GetParent(testParent.GetSourceElement()));
                    if (testParent != null &&
                        (int)testParent.Current.ProcessId > 0 &&
                        testParent != UiElement.RootElement) {
                        
                        ancestors.Add(testParent);
                    }
                }
                return ancestors.ToArray();
            } catch {
                return ancestors.ToArray();
            }
        }
        #endregion get the parent or an ancestor
        
        #region Collect ancestors
        /// <summary>
        ///
        /// </summary>
        /// <param name="cmdlet"></param>
        /// <param name="element"></param>
        internal static void CollectAncestors(this IUiElement element, TranscriptCmdletBase cmdlet, ref string errorMessage, ref bool errorOccured)
        {
            TreeWalker walker =
                new TreeWalker(
                    System.Windows.Automation.Condition.TrueCondition);
            
            try
            {
                // commented out 201206210
                //testparent =
                //    walker.GetParent(element);
                IUiElement testParent = element;

                while (testParent != null && (int)testParent.Current.ProcessId > 0) {
                    
                    testParent =
                        AutomationFactory.GetUiElement(walker.GetParent(testParent.GetSourceElement()));
                    
                    if (testParent == null || (int) testParent.Current.ProcessId <= 0) continue;
                    if (testParent == cmdlet.OddRootElement)
                    { testParent = null; }
                    else{
                        string parentControlType =
                            // getControlTypeNameOfAutomationElement(testparent, element);
                            // testparent.Current.ControlType.ProgrammaticName.Substring(
                            // element.Current.ControlType.ProgrammaticName.IndexOf('.') + 1);
                            //  // experimental
                            testParent.Current.ControlType.ProgrammaticName.Substring(
                                testParent.Current.ControlType.ProgrammaticName.IndexOf('.') + 1);
                        //  // if (parentControlType.Length == 0) {
                        // break;
                        //}
                            
                        // in case this element is an upper-level Pane
                        // residing directrly under the RootElement
                        // change type to window
                        // i.e. Get-UiaPane - >  Get-UiaWindow
                        // since Get-UiaPane is unable to get something more than
                        // a window's child pane control
                        if (parentControlType == "Pane" || parentControlType == "Menu") {
                            
                            // 20131109
                            //if (walker.GetParent(testParent) == cmdlet.rootElement) {
                            // 20131112
                            //if ((new UiElement(walker.GetParent(testParent.SourceElement))) == cmdlet.oddRootElement) {
                            // 20131118
                            // property to method
                            //if (ObjectsFactory.GetUiElement(walker.GetParent(testParent.SourceElement)) == cmdlet.oddRootElement) {
                            if (AutomationFactory.GetUiElement(walker.GetParent(testParent.GetSourceElement())) == cmdlet.OddRootElement) {
                                parentControlType = "Window";
                            }
                        }
                            
                        string parentVerbosity =
                            @"Get-Uia" + parentControlType;
                        try {
                            if (testParent.Current.AutomationId.Length > 0) {
                                parentVerbosity += (" -AutomationId '" + testParent.Current.AutomationId + "'");
                            }
                        }
                        catch {
                        }
                        if (!cmdlet.NoClassInformation) {
                            try {
                                if (testParent.Current.ClassName.Length > 0) {
                                    parentVerbosity += (" -Class '" + testParent.Current.ClassName + "'");
                                }
                            }
                            catch {
                            }
                        }
                        try {
                            if (testParent.Current.Name.Length > 0) {
                                parentVerbosity += (" -Name '" + testParent.Current.Name + "'");
                            }
                        }
                        catch {
                        }

                        if (cmdlet.LastRecordedItem[cmdlet.LastRecordedItem.Count - 1].ToString() == parentVerbosity)
                            continue;
                        cmdlet.LastRecordedItem.Add(parentVerbosity);
                        cmdlet.WriteVerbose(parentVerbosity);
                    }
                }
            }
            catch (Exception eErrorInTheInnerCycle) {
                cmdlet.WriteDebug(cmdlet, eErrorInTheInnerCycle.Message);
                // _errorMessageInTheInnerCycle =
                errorMessage =
                    eErrorInTheInnerCycle.Message;
                // _errorInTheInnerCycle = true;
                errorOccured = true;
            }
        }
        #endregion Collect ancestors
        
        /// <summary>
        /// Retrievs an element's ControlType property as a string.
        /// </summary>
        /// <param name="element">AutomationElement</param>
        /// <returns>string</returns>
        internal static string GetElementControlTypeString(this IUiElement element)
        {
            string elementControlType = String.Empty;
            try {
                elementControlType = element.Current.ControlType.ProgrammaticName;
            } catch {
                elementControlType = element.Cached.ControlType.ProgrammaticName;
            }
            if (string.Empty != elementControlType && 0 < elementControlType.Length) {
                elementControlType = elementControlType.Substring(elementControlType.IndexOf('.') + 1);
            }
            //string elementVerbosity = String.Empty;
            //if (string.Empty == elementControlType || 0 == elementControlType.Length) {
            //    return result;
            //}
            return elementControlType;
        }
        
        /// <summary>
        /// Retrieves such element's properties as AutomationId, Name, Class(Name) and Value
        /// </summary>
        /// <param name="cmdlet">cmdlet to report</param>
        /// <param name="element">The element properties taken from</param>
        /// <param name="propertyName">The name of property</param>
        /// <param name="pattern">an object of the ValuePattern type</param>
        /// <param name="hasName">an object has Name</param>
        /// <returns></returns>
        internal static string GetElementPropertyString(
            this IUiElement element,
            PSCmdletBase cmdlet,
            string propertyName,
            IMySuperValuePattern pattern,
            ref bool hasName)
        {
            cmdlet.WriteVerbose(cmdlet, "getting " + propertyName);
            string tempString = string.Empty;
            try {
                
                switch (propertyName) {
                    case "Name":
                        if (0 < element.Current.Name.Length) {
                            tempString = element.Current.Name;
                            hasName = true;
                        }
                        break;
                    case "AutomationId":
                        if (0 < element.Current.AutomationId.Length) {
                            tempString = element.Current.AutomationId;
                        }
                        break;
                    case "Class":
                        if (0 < element.Current.ClassName.Length) {
                            tempString = element.Current.ClassName;
                        }
                        break;
                    case "Value":
                        if (!string.IsNullOrEmpty(pattern.Current.Value)) {
                            tempString = pattern.Current.Value;
                            hasName = true;
                        }
                        break;
                    case "Win32":
                        if (0 < element.Current.NativeWindowHandle) {
                            tempString = ".";
                        }
                        break;
                    default:
                        
                    	break;
                }
            } catch {
                switch (propertyName) {
                    case "Name":
                        if (0 < element.Cached.Name.Length) {
                            tempString = element.Cached.Name;
                            hasName = true;
                        }
                        break;
                    case "AutomationId":
                        if (0 < element.Cached.AutomationId.Length) {
                            tempString = element.Cached.AutomationId;
                        }
                        break;
                    case "Class":
                        if (0 < element.Cached.ClassName.Length) {
                            tempString = element.Cached.ClassName;
                        }
                        break;
                    case "Value":
                        if (!string.IsNullOrEmpty(pattern.Cached.Value)) {
                            tempString = pattern.Cached.Value;
                            hasName = true;
                        }
                        break;
                    case "Win32":
                        if (0 < element.Cached.NativeWindowHandle) {
                            tempString = ".";
                        }
                        break;
                    default:
                        
                    	break;
                }
            }
            if (string.IsNullOrEmpty(tempString)) {
                return string.Empty;
            } else {
                if ("Win32" == propertyName) {
                    tempString =
                        " -" + propertyName;
                } else {
                    tempString =
                        " -" + propertyName + " '" + tempString + "'";
                }
                return tempString;
            }
        }
        
        internal static List<IUiElement> GetControlByNameViaWin32(
            this IUiElement containerElement,
            GetControlCmdletBase cmdlet,
            string controlTitle,
            string controlValue)
        {
            List<IUiElement> resultCollection = new List<IUiElement>();
            
            cmdlet.WriteVerbose(cmdlet, "checking the container control");

            if (null == containerElement) { return resultCollection; }
            cmdlet.WriteVerbose(cmdlet, "checking the Name parameter");
            
            controlTitle = string.IsNullOrEmpty(controlTitle) ? "*" : controlTitle;
            controlValue = string.IsNullOrEmpty(controlValue) ? "*" : controlValue;
            
            try {
                IntPtr containerHandle =
                    new IntPtr(containerElement.Current.NativeWindowHandle);
                if (containerHandle == IntPtr.Zero){
                    cmdlet.WriteVerbose(cmdlet, "The container control has no handle");

                    return resultCollection;
                }
                
                List<IntPtr> handlesCollection =
                    UiaHelper.GetControlByNameViaWin32Recursively(cmdlet, containerHandle, controlTitle, 1);
                
                const WildcardOptions options =
                    WildcardOptions.IgnoreCase |
                    WildcardOptions.Compiled;
                
                WildcardPattern wildcardName =
                    new WildcardPattern(controlTitle, options);
                WildcardPattern wildcardValue =
                    new WildcardPattern(controlValue, options);
                
                if (null == handlesCollection || 0 == handlesCollection.Count) return resultCollection;
                cmdlet.WriteVerbose(cmdlet, "handles.Count = " + handlesCollection.Count.ToString());
                
                foreach (IntPtr controlHandle in handlesCollection) {
                    try {
                        cmdlet.WriteVerbose(cmdlet, "checking a handle");
                        if (IntPtr.Zero == controlHandle) continue;
                        cmdlet.WriteVerbose(cmdlet, "the handle is not null");
                        
                        IUiElement tempElement =
                            UiElement.FromHandle(controlHandle);
                        cmdlet.WriteVerbose(cmdlet, "adding the handle to the collection");
                                
                        cmdlet.WriteVerbose(cmdlet, controlTitle);
                        cmdlet.WriteVerbose(cmdlet, tempElement.Current.Name);
                        
                        if (tempElement.IsMatchWildcardPattern(cmdlet, resultCollection, wildcardName, tempElement.Current.Name)) continue;
                        if (tempElement.IsMatchWildcardPattern(cmdlet, resultCollection, wildcardName, tempElement.Current.AutomationId)) continue;
                        if (tempElement.IsMatchWildcardPattern(cmdlet, resultCollection, wildcardName, tempElement.Current.ClassName)) continue;
                        try {
                            string elementValue =
                                // 20131208
                                // (tempElement.GetCurrentPattern(ValuePattern.Pattern) as ValuePattern).Current.Value;
                                // (tempElement.GetCurrentPattern<IMySuperValuePattern, ValuePattern>(ValuePattern.Pattern) as ValuePattern).Current.Value;
                                tempElement.GetCurrentPattern<IMySuperValuePattern>(ValuePattern.Pattern).Current.Value;
                            if (tempElement.IsMatchWildcardPattern(cmdlet, resultCollection, wildcardName, elementValue)) continue;
                            if (tempElement.IsMatchWildcardPattern(cmdlet, resultCollection, wildcardValue, elementValue)) continue;
                        }
                        catch { //(Exception eValuePattern) {
                        }
                    }
                    catch (Exception eGetAutomationElementFromHandle) {
                        cmdlet.WriteVerbose(cmdlet, eGetAutomationElementFromHandle.Message);
                    }
                }
                return resultCollection;
            } catch (Exception eWin32Control) {
                cmdlet.WriteVerbose(cmdlet, "UiaHelper.GetControlByName() failed");
                cmdlet.WriteVerbose(cmdlet, eWin32Control.Message);
                return resultCollection;
            }
        }
        
        internal static bool IsMatchWildcardPattern(
            this IUiElement elementInput,
            PSCmdletBase cmdlet,
            IList resultCollection,
            WildcardPattern wcPattern,
            string dataToCheck)
        {
            bool result = false;
            
            if (string.IsNullOrEmpty(dataToCheck)) {
                return result;
            }
            
            if (!wcPattern.IsMatch(dataToCheck)) return result;
            
            result = true;
            cmdlet.WriteVerbose(cmdlet, "name '" + dataToCheck + "' matches!");
            resultCollection.Add(elementInput);
            
            return result;
        }
        
        #region Patterns
        #region InvokePattern
        public static IUiElement PerformClick(this IUiElement element)
        {
            try {
                element.GetCurrentPattern<IMySuperInvokePattern>(InvokePattern.Pattern).Invoke();
            }
            catch {
                // click via Win32
            }
            return element;
        }
        
        public static IUiElement PerformDoubleClick(this IUiElement element)
        {
            HasControlInputCmdletBase cmdlet =
                new HasControlInputCmdletBase();
            cmdlet.ClickControl(
                cmdlet,
                element,
                false,
                false,
                false,
                false,
                false,
                false,
                true,
                50,
                Preferences.ClickOnControlByCoordX,
                Preferences.ClickOnControlByCoordY);
            
            return element;
        }
        #endregion InvokePattern
        
        #region SelectionItemPattern
        public static IUiElement PerformSelect(this IUiElement element)
        {
            try {
                element.GetCurrentPattern<IMySuperSelectionItemPattern>(SelectionItemPattern.Pattern).Select();
            }
            catch {
                //
            }
            return element;
        }
        
        public static IUiElement PerformAddToSelection(this IUiElement element)
        {
            try {
                element.GetCurrentPattern<IMySuperSelectionItemPattern>(SelectionItemPattern.Pattern).AddToSelection();
            }
            catch {
                // 
            }
            return element;
        }
        
        public static IUiElement PerformRemoveFromSelection(this IUiElement element)
        {
            try {
                element.GetCurrentPattern<IMySuperSelectionItemPattern>(SelectionItemPattern.Pattern).RemoveFromSelection();
            }
            catch {
                // 
            }
            return element;
        }
        
        public static bool GetIsSelected(this IUiElement element)
        {
            try {
                return element.GetCurrentPattern<IMySuperSelectionItemPattern>(SelectionItemPattern.Pattern).Current.IsSelected;
            }
            catch {
                //
            }
            return false;
        }
        
        public static IUiElement GetSelectionContainer(this IUiElement element)
        {
            try {
                return element.GetCurrentPattern<IMySuperSelectionItemPattern>(SelectionItemPattern.Pattern).Current.SelectionContainer;
            } catch (Exception) {
                // 
                // throw;
            }
            return null;
        }
        #endregion SelectionItemPattern
        #region SelectionPattern
        public static IUiElement[] PerformGetSelection(this IUiElement element)
        {
            try {
                return element.GetCurrentPattern<IMySuperSelectionPattern>(SelectionPattern.Pattern).Current.GetSelection();
            }
            catch {
                // 
            }
            return new UiElement[] {};
        }
        
        public static bool GetCanSelectMultiple(this IUiElement element)
        {
            try {
                return element.GetCurrentPattern<IMySuperSelectionPattern>(SelectionPattern.Pattern).Current.CanSelectMultiple;
            } catch (Exception) {
                // 
                // throw;
            }
            return false;
        }
        
        public static bool GetIsSelectionRequired(this IUiElement element)
        {
            try {
                return element.GetCurrentPattern<IMySuperSelectionPattern>(SelectionPattern.Pattern).Current.IsSelectionRequired;
            } catch (Exception) {
                // 
                // throw;
            }
            return false;
        }
        #endregion SelectionPattern
        #region TogglePattern
        public static IUiElement PerformToggle(this IUiElement element)
        {
            try {
                element.GetCurrentPattern<IMySuperTogglePattern>(TogglePattern.Pattern).Toggle();
            }
            catch {
                // maybe, a click
            }
            return element;
        }
        #endregion TogglePattern
        #region TransformPattern
        public static IUiElement PerformMove(this IUiElement element, double x, double y)
        {
            try {
                element.GetCurrentPattern<IMySuperTransformPattern>(TransformPattern.Pattern).Move(x, y);
            } catch (Exception) {
                // 
                // throw;
            }
            return element;
        }
		
		public static IUiElement PerformResize(this IUiElement element, double width, double height)
		{
		    try {
		        element.GetCurrentPattern<IMySuperTransformPattern>(TransformPattern.Pattern).Resize(width, height);
		    } catch (Exception) {
		        // 
		        // throw;
		    }
		    return element;
		}
		
		public static IUiElement PerformRotate(this IUiElement element, double degrees)
		{
		    try {
		        element.GetCurrentPattern<IMySuperTransformPattern>(TransformPattern.Pattern).Rotate(degrees);
		    } catch (Exception) {
		        // 
		        // throw;
		    }
		    return element;
		}
        #endregion TransformPattern
        #region ValuePattern
        public static IUiElement PerformSetValue(this IUiElement element, string value)
        {
            try {
                element.GetCurrentPattern<IMySuperValuePattern>(ValuePattern.Pattern).SetValue(value);
            } catch (Exception) {
                // 
                // throw;
            }
            return element;
        }
        
        public static string PerformGetValue(this IUiElement element)
        {
            try {
                return element.GetCurrentPattern<IMySuperValuePattern>(ValuePattern.Pattern).Current.Value;
            } catch (Exception) {
                // 
                // throw;
            }
            return string.Empty;
        }
        
        public static bool GetIsReadOnly(this IUiElement element)
        {
            try {
                return element.GetCurrentPattern<IMySuperValuePattern>(ValuePattern.Pattern).Current.IsReadOnly;
            } catch (Exception) {
                // 
                // throw;
            }
            return false;
        }
        #endregion ValuePattern
        #region WindowPattern
        public static IUiElement PerformSetWindowVisualState(this IUiElement element, WindowVisualState state)
        {
            try {
                element.GetCurrentPattern<IMySuperWindowPattern>(WindowPattern.Pattern).SetWindowVisualState(state);
            } catch (Exception) {
                // 
                // throw;
            }
            return element;
        }
        
		public static IUiElement PerformClose(this IUiElement element)
		{
		    try {
		        element.GetCurrentPattern<IMySuperWindowPattern>(WindowPattern.Pattern).Close();
		    } catch (Exception) {
		        // 
		        // throw;
		    }
		    return element;
		}
		
		public static bool PerformWaitForInputIdle(this IUiElement element, int milliseconds)
		{
		    try {
		        return element.GetCurrentPattern<IMySuperWindowPattern>(WindowPattern.Pattern).WaitForInputIdle(milliseconds);
		    } catch (Exception) {
		        // 
		        // throw;
		    }
		    return false;
		}
		
		public static bool GetCanMaximize(this IUiElement element)
		{
		    try {
		        return element.GetCurrentPattern<IMySuperWindowPattern>(WindowPattern.Pattern).Current.CanMaximize;
		    } catch (Exception) {
		        // 
		        // throw;
		    }
		    return false;
		}
		
		public static bool GetCanMinimize(this IUiElement element)
		{
		    try {
		        return element.GetCurrentPattern<IMySuperWindowPattern>(WindowPattern.Pattern).Current.CanMinimize;
		    } catch (Exception) {
		        // 
		        // throw;
		    }
		    return false;
		}
		
		public static bool GetIsModal(this IUiElement element)
		{
		    try {
		        return element.GetCurrentPattern<IMySuperWindowPattern>(WindowPattern.Pattern).Current.IsModal;
		    } catch (Exception) {
		        // 
		        // throw;
		    }
		    return false;
		}
		
		public static bool GetIsTopmost(this IUiElement element)
		{
		    try {
		        return element.GetCurrentPattern<IMySuperWindowPattern>(WindowPattern.Pattern).Current.IsTopmost;
		    } catch (Exception) {
		        // 
		        // throw;
		    }
		    return false;
		}
		
		public static WindowInteractionState GetWindowInteractionState(this IUiElement element)
		{
		    try {
		        return element.GetCurrentPattern<IMySuperWindowPattern>(WindowPattern.Pattern).Current.WindowInteractionState;
		    } catch (Exception) {
		        // 
		        // throw;
		    }
		    return WindowInteractionState.NotResponding;
		}
		
		public static WindowVisualState GetWindowVisualState(this IUiElement element)
		{
		    try {
		        return element.GetCurrentPattern<IMySuperWindowPattern>(WindowPattern.Pattern).Current.WindowVisualState;
		    } catch (Exception) {
		        // 
		        // throw;
		    }
		    return WindowVisualState.Normal;
		}
        #endregion WindowPattern
        #endregion Patterns
    }
}