using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.UI;
using TMPro;
using UnityEngine;

////TODO: localization support

////TODO: deal with composites that have parts bound in different control schemes

namespace UnityEngine.InputSystem.Samples.RebindUI
{
    /// <summary>
    /// A reusable component with a self-contained UI for rebinding a single action.
    /// </summary>
    /// 
    
    public class RebindActionUI : MonoBehaviour
    {
        /// <summary>
        /// Reference to the action that is to be rebound.
        /// </summary>
        public InputActionReference actionReference
        {
            get => m_Action;
            set
            {
                m_Action = value;
                UpdateActionLabel();
                UpdateBindingDisplay();
            }
        }

        /// <summary>
        /// ID (in string form) of the binding that is to be rebound on the action.
        /// </summary>
        /// <seealso cref="InputBinding.id"/>
        public string bindingId
        {
            get => m_BindingId;
            set
            {
                m_BindingId = value;
                UpdateBindingDisplay();
            }
        }

        public InputBinding.DisplayStringOptions displayStringOptions
        {
            get => m_DisplayStringOptions;
            set
            {
                m_DisplayStringOptions = value;
                UpdateBindingDisplay();
            }
        }

        /// <summary>
        /// Text component that receives the name of the action. Optional.
        /// </summary>
        public TextMeshProUGUI actionLabel
        {
            get => m_ActionLabel;
            set
            {
                m_ActionLabel = value;
                UpdateActionLabel();
            }
        }

        /// <summary>
        /// Text component that receives the display string of the binding. Can be <c>null</c> in which
        /// case the component entirely relies on <see cref="updateBindingUIEvent"/>.
        /// </summary>
        public TextMeshProUGUI bindingText
        {
            get => m_BindingText;
            set
            {
                m_BindingText = value;
                UpdateBindingDisplay();
            }
        }

        /// <summary>
        /// Optional text component that receives a text prompt when waiting for a control to be actuated.
        /// </summary>
        /// <seealso cref="startRebindEvent"/>
        /// <seealso cref="rebindOverlay"/>
        public TextMeshProUGUI rebindPrompt
        {
            get => m_RebindText;
            set => m_RebindText = value;
        }

        /// <summary>
        /// Optional UI that is activated when an interactive rebind is started and deactivated when the rebind
        /// is finished. This is normally used to display an overlay over the current UI while the system is
        /// waiting for a control to be actuated.
        /// </summary>
        /// <remarks>
        /// If neither <see cref="rebindPrompt"/> nor <c>rebindOverlay</c> is set, the component will temporarily
        /// replaced the <see cref="bindingText"/> (if not <c>null</c>) with <c>"Waiting..."</c>.
        /// </remarks>
        /// <seealso cref="startRebindEvent"/>
        /// <seealso cref="rebindPrompt"/>
        public GameObject rebindOverlay
        {
            get => m_RebindOverlay;
            set => m_RebindOverlay = value;
        }

        /// <summary>
        /// Event that is triggered every time the UI updates to reflect the current binding.
        /// This can be used to tie custom visualizations to bindings.
        /// </summary>
        public UpdateBindingUIEvent updateBindingUIEvent
        {
            get
            {
                if (m_UpdateBindingUIEvent == null)
                    m_UpdateBindingUIEvent = new UpdateBindingUIEvent();
                return m_UpdateBindingUIEvent;
            }
        }

        /// <summary>
        /// Event that is triggered when an interactive rebind is started on the action.
        /// </summary>
        public InteractiveRebindEvent startRebindEvent
        {
            get
            {
                if (m_RebindStartEvent == null)
                    m_RebindStartEvent = new InteractiveRebindEvent();
                return m_RebindStartEvent;
            }
        }

        /// <summary>
        /// Event that is triggered when an interactive rebind has been completed or canceled.
        /// </summary>
        public InteractiveRebindEvent stopRebindEvent
        {
            get
            {
                if (m_RebindStopEvent == null)
                    m_RebindStopEvent = new InteractiveRebindEvent();
                return m_RebindStopEvent;
            }
        }

        /// <summary>
        /// When an interactive rebind is in progress, this is the rebind operation controller.
        /// Otherwise, it is <c>null</c>.
        /// </summary>
        public InputActionRebindingExtensions.RebindingOperation ongoingRebind => m_RebindOperation;

        /// <summary>
        /// Return the action and binding index for the binding that is targeted by the component
        /// according to
        /// </summary>
        /// <param name="action"></param>
        /// <param name="bindingIndex"></param>
        /// <returns></returns>
        public bool ResolveActionAndBinding(out InputAction action, out int bindingIndex)
        {
            bindingIndex = -1;

            action = m_Action?.action;
            if (action == null)
                return false;

            if (string.IsNullOrEmpty(m_BindingId))
                return false;

            // Look up binding index.
            var bindingId = new Guid(m_BindingId);
            bindingIndex = action.bindings.IndexOf(x => x.id == bindingId);
            if (bindingIndex == -1)
            {
                Debug.LogError($"Cannot find binding with ID '{bindingId}' on '{action}'", this);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Trigger a refresh of the currently displayed binding.
        /// </summary>
        public void UpdateBindingDisplay()
        {
            var displayString = string.Empty;
            var deviceLayoutName = default(string);
            var controlPath = default(string);

            // Get display string from action.
            var action = m_Action?.action;
            if (action != null)
            {
                var bindingIndex = action.bindings.IndexOf(x => x.id.ToString() == m_BindingId);
                if (bindingIndex != -1)
                    displayString = action.GetBindingDisplayString(bindingIndex, out deviceLayoutName, out controlPath, displayStringOptions);
               
                if (displayString.ToLower().Contains("num") && action.ToString().ToLower().Contains("arrow"))
                {
                    //Debug.Log("UpdateBindingDisplay: numbpad in display and keyboard in action. Action: " + action.ToString() + " displayString: " + displayString + " deviceLayoutName: " + deviceLayoutName + " displayStringOptions: " + displayStringOptions + " controlPath: " + controlPath);

                    // Replace the num strings with converted to arrow keys
                    // check if a "/" exits in the string, if not, replace string with arrow equivalent
                    if (displayString.Contains("/"))
                    {
                        //Debug.Log("UpdateBindingDisplay: YES SLASH");
                        string[] brokenUpDisplayString = displayString.Split("/");
                        if (brokenUpDisplayString.Length > 0)
                        {
                            for (int i = 0; i < brokenUpDisplayString.Length; i++)
                            {
                                string newString = brokenUpDisplayString[i];
                                if (newString.ToLower().Contains("num 8"))
                                    newString = "Up Arrow";
                                else if (newString.ToLower().Contains("num 2"))
                                    newString = "Down Arrow";
                                else if (newString.ToLower().Contains("num 4"))
                                    newString = "Left Arrow";
                                else if (newString.ToLower().Contains("num 6"))
                                    newString = "Right Arrow";
                                brokenUpDisplayString[i] = newString;
                            }
                            displayString = brokenUpDisplayString[0];
                            if (brokenUpDisplayString.Length > 1)
                            {
                                for (int i = 1; i < brokenUpDisplayString.Length; i++)
                                {
                                    displayString += "/";
                                    displayString += brokenUpDisplayString[i];
                                }
                            }
                        }
                    }
                    else
                    {
                        //Debug.Log("UpdateBindingDisplay: no slash");
                        if (displayString.ToLower().Contains("num 8"))
                            displayString = "Up Arrow";
                        else if (displayString.ToLower().Contains("num 2"))
                            displayString = "Down Arrow";
                        else if (displayString.ToLower().Contains("num 4"))
                            displayString = "Left Arrow";
                        else if (displayString.ToLower().Contains("num 6"))
                            displayString = "Right Arrow";

                    }
                    // if there is a "/", break the string on the slash. Then, check it part of the string, convert num to arrow key, then rebuild back to the display string?
                }
            }

            // Set on label (if any).
            if (m_BindingText != null)
                m_BindingText.text = displayString;
            //Debug.Log("UpdateBindingDisplay: " + displayString);

            // Give listeners a chance to configure UI in response.
            m_UpdateBindingUIEvent?.Invoke(this, displayString, deviceLayoutName, controlPath);
        }

        /// <summary>
        /// Remove currently applied binding overrides.
        /// </summary>
        public void ResetToDefault()
        {
            if (!ResolveActionAndBinding(out var action, out var bindingIndex))
                return;

            if (action.bindings[bindingIndex].isComposite)
            {
                // It's a composite. Remove overrides from part bindings.
                for (var i = bindingIndex + 1; i < action.bindings.Count && action.bindings[i].isPartOfComposite; ++i)
                {
                    action.RemoveBindingOverride(i);
                    Debug.Log("ResetToDefault: for action: " + action.ToString() + " with binding index of: " + i.ToString() + " and resetting the binding path to its default path of: " + action.bindings[i].path + " and is it a composite? " + action.bindings[i].isComposite + ":" + action.bindings[i].isPartOfComposite);
                    CheckDuplicateBindings(action, i, action.bindings[i].isComposite);
                }
                    
            }
            else
            {
                action.RemoveBindingOverride(bindingIndex);
                Debug.Log("ResetToDefault: for action: " + action.ToString() + " with binding index of: " + bindingIndex.ToString() + " and resetting the binding path to its default path of: " + action.bindings[bindingIndex].path + " and is it a composite? " + action.bindings[bindingIndex].isComposite + ":" + action.bindings[bindingIndex].isPartOfComposite);
                CheckDuplicateBindings(action, bindingIndex, action.bindings[bindingIndex].isComposite);
            }
            UpdateBindingDisplay();
        }

        /// <summary>
        /// Initiate an interactive rebind that lets the player actuate a control to choose a new binding
        /// for the action.
        /// </summary>
        public void StartInteractiveRebind()
        {
            if (!ResolveActionAndBinding(out var action, out var bindingIndex))
                return;

            // If the binding is a composite, we need to rebind each part in turn.
            if (action.bindings[bindingIndex].isComposite)
            {
                var firstPartIndex = bindingIndex + 1;
                if (firstPartIndex < action.bindings.Count && action.bindings[firstPartIndex].isPartOfComposite)
                    PerformInteractiveRebind(action, firstPartIndex, allCompositeParts: true);
            }
            else
            {
                PerformInteractiveRebind(action, bindingIndex);
            }
        }

        private void PerformInteractiveRebind(InputAction action, int bindingIndex, bool allCompositeParts = false)
        {
            m_RebindOperation?.Cancel(); // Will null out m_RebindOperation.

            void CleanUp()
            {
                m_RebindOperation?.Dispose();
                m_RebindOperation = null;
            }
            // disable the action before use

            // Configure the rebind.
            m_RebindOperation = action.PerformInteractiveRebinding(bindingIndex)
                .WithTimeout(5f)
                .WithControlsExcluding("<Gamepad>/start")
                .WithControlsExcluding("<Gamepad>/select")
                .WithControlsExcluding("<Gamepad>/leftStick")
                .WithControlsExcluding("<Gamepad>/rightStick")
                .WithCancelingThrough("<Keyboard>/escape")
                .OnCancel(
                    operation =>
                    {
                        m_RebindStopEvent?.Invoke(this, operation);
                        m_RebindOverlay?.SetActive(false);
                        UpdateBindingDisplay();
                        CleanUp();
                    })
                .OnComplete(
                    operation =>
                    {
                        m_RebindOverlay?.SetActive(false);
                        m_RebindStopEvent?.Invoke(this, operation);
                        // Check for duplicate bindings?
                        if (CheckDuplicateBindings(action, bindingIndex, allCompositeParts))
                        {
                            action.RemoveBindingOverride(bindingIndex);
                            CleanUp();
                            //PerformInteractiveRebind(action, bindingIndex, allCompositeParts);
                            return;
                        }
                        //Debug.Log("New binding effective path: " + action.bindings[bindingIndex].effectivePath + " and override path " + action.bindings[bindingIndex].overridePath + " groups: " + action.bindings[bindingIndex].groups.ToString());
                        
                        UpdateBindingDisplay();
                        CleanUp();

                        // If there's more composite parts we should bind, initiate a rebind
                        // for the next part.
                        if (allCompositeParts)
                        {
                            var nextBindingIndex = bindingIndex + 1;
                            if (nextBindingIndex < action.bindings.Count && action.bindings[nextBindingIndex].isPartOfComposite)
                                PerformInteractiveRebind(action, nextBindingIndex, true);
                        }
                    });

            // If it's a part binding, show the name of the part in the UI.
            var partName = default(string);
            if (action.bindings[bindingIndex].isPartOfComposite)
                partName = $"Binding '{action.bindings[bindingIndex].name}'. ";

            // Bring up rebind overlay, if we have one.
            m_RebindOverlay?.SetActive(true);
            if (m_RebindText != null)
            {
                var text = !string.IsNullOrEmpty(m_RebindOperation.expectedControlType)
                    ? $"{partName}Waiting for {m_RebindOperation.expectedControlType} input..."
                    : $"{partName}Waiting for input...";
                m_RebindText.text = text;
            }

            // If we have no rebind overlay and no callback but we have a binding text label,
            // temporarily set the binding text label to "<Waiting>".
            if (m_RebindOverlay == null && m_RebindText == null && m_RebindStartEvent == null && m_BindingText != null)
                m_BindingText.text = "<Waiting...>";

            // Give listeners a chance to act on the rebind starting.
            m_RebindStartEvent?.Invoke(this, m_RebindOperation);

            m_RebindOperation.Start();
        }
        private bool CheckDuplicateBindings(InputAction action, int bindingIndex, bool allCompositeParts = false)
        {
            InputBinding newBinding = action.bindings[bindingIndex];
            Debug.Log("CheckDuplicateBindings: allCompositeParts is: " + allCompositeParts.ToString() + " action is: " + newBinding.action.ToString() + " action map is: " + action.actionMap.ToString() + " new binding effective path is: " + newBinding.effectivePath.ToString() + " new binding group/control sheme is: " + newBinding.groups.ToString());
            //string oldBindingPath = null;

            // Check through each action map in ResetAllBindings list of action maps?
            foreach (InputActionMap map in ResetAllBindings.instance.actionMapsToCheck)
            {
                bool breakLoop = false;
                foreach (InputBinding binding in map.bindings)
                {
                    if (binding.action == newBinding.action)
                    {
                        //oldBindingPath = binding.effectivePath;
                        continue;
                    }
                    if (binding.effectivePath == newBinding.effectivePath)
                    {

                        //InputAction dupAction = playerControlsReference.FindAction(binding.action);
                        //int dupBindingIndex = dupAction.GetBindingIndex(binding);
                        InputAction dupAction = this.GetComponent<StoreControls>().PlayerControls.FindAction(binding.action);
                        int dupBindingIndex = dupAction.GetBindingIndex(binding.groups, binding.path);

                        Debug.Log("CheckDuplicateBindings: Duplicate binding found. new binding path: " + newBinding.effectivePath.ToString() + " duplicate effective path: " + binding.effectivePath.ToString() + " duplicate action is: " + binding.action.ToString() + " new binding PATH is: " + newBinding.path.ToString() + " is new binding a composite: " + newBinding.isComposite.ToString());
                        Debug.Log("CheckDuplicateBindings: new action: " + action.ToString() + " new binding index: " + bindingIndex + " duplicate action: " + dupAction.ToString() + " duplicate binding index: " + dupBindingIndex.ToString() + " does the duplicate have overrides? " + binding.hasOverrides.ToString() + " is duplicate action a composite: " + dupAction.bindings[dupBindingIndex].isComposite.ToString() + ":" + binding.isComposite);
                        //PerformInteractiveRebind(dupAction, dupBindingIndex, false);

                        // Check for what value the duplicate binding will be swapped to
                        // First, check if the duplicate binding has any overrides already. If it does, and the default path is not already assigned, revert to default path.
                        bool wasDuplicateChanged = false;
                        if (binding.hasOverrides && !wasDuplicateChanged)
                        {
                            Debug.Log("CheckDuplicateBindings: duplicate path has overrides. Duplicate action: " + binding.action.ToString() + " Override path: " + binding.overridePath + " default path: " + binding.path);
                            bool isDefaultPathAlreadyUsed = CheckIfPathExistsInActions(dupAction, dupBindingIndex, binding.path);
                            if (!isDefaultPathAlreadyUsed)
                            {
                                Debug.Log("CheckDuplicateBindings: duplicate path has overrides and the default path is available. Reseting to default path: " + binding.path + " for action: " + binding.action.ToString());
                                dupAction.RemoveBindingOverride(dupBindingIndex);
                                wasDuplicateChanged = true;
                            }
                            else
                            {
                                Debug.Log("CheckDuplicateBindings: duplicate path has overrides AND THE DEFAULT PATH IS NOT AVAILABLE. Default path: " + binding.path + " for action: " + binding.action.ToString());
                            }
                        }
                        // If the duplicate's does not have any overrides, check if the new binding's default path is available
                        if (!wasDuplicateChanged)
                        {
                            Debug.Log("CheckDuplicateBindings: duplicate path was not changed to its default path. Checking if the new binding's default path is available. New binding default path: " + newBinding.path + " new binding action: " + newBinding.action.ToString());
                            bool isNewBindingDefaultPathAlreadyUsed = CheckIfPathExistsInActions(dupAction, dupBindingIndex, newBinding.path);
                            if (!isNewBindingDefaultPathAlreadyUsed)
                            {
                                Debug.Log("CheckDuplicateBindings: new binding default path is available!. Duplicate action: " + binding.action.ToString() + " the NEW BINDING's default path is available. new binding path: " + newBinding.path + " new binding action: " + newBinding.action.ToString());
                                dupAction.ApplyBindingOverride(dupBindingIndex, newBinding.path);
                                wasDuplicateChanged = true;
                            }
                            else
                            {
                                Debug.Log("CheckDuplicateBindings: new binding default path IS NOT AVAILABLE. Duplicate action: " + binding.action.ToString() + " the NEW BINDING's default path IS NOT AVAILABLE. new binding path: " + newBinding.path + " new binding action: " + newBinding.action.ToString());
                                // The duplicate cannot switch to their own default, or the new bindings default. First, find what binding is using the new bindings default path
                                InputBinding secondDupBinding = CheckWhatActionIsUsingPath(binding, dupBindingIndex, newBinding.path);
                                if (secondDupBinding != binding)
                                {
                                    Debug.Log("CheckDuplicateBindings: new binding default path IS NOT AVAILABLE. The action with the new binding default path is: " + secondDupBinding.action.ToString());
                                    // Check if the second duplicate's default value is available
                                    InputAction secondDupAction = this.GetComponent<StoreControls>().PlayerControls.FindAction(secondDupBinding.action);
                                    int secondDupBindingIndex = secondDupAction.GetBindingIndex(secondDupBinding.groups, secondDupBinding.path);
                                    bool isSecondDupDefaultPathAlreadyUsed = CheckIfPathExistsInActions(secondDupAction, secondDupBindingIndex, secondDupBinding.path);
                                    if (!isSecondDupDefaultPathAlreadyUsed)
                                    {
                                        Debug.Log("CheckDuplicateBindings: the second duplicate default path is available! Setting the new path for: " + dupAction.ToString() + " to a path of: " + secondDupBinding.path);
                                        dupAction.ApplyBindingOverride(dupBindingIndex, secondDupBinding.path);
                                        wasDuplicateChanged = true;
                                    }
                                    else
                                    {
                                        Debug.Log("CheckDuplicateBindings: the second duplicate default path IS NOT AVAILABLE! Second duplicate default path is: " + secondDupBinding.path);
                                        // So far, the duplicate cannot be reverted to their original default path
                                        // the new binding's default path is also in use, so the duplicate cannot use that
                                        // the second duplicate that was using the new binding's default path was found. The second duplicate's default path was also in use and cannot be changed.
                                        // So now, all bindings will be looped through. The first binding with a default path that is not in use will be used for the duplicate's bew path?
                                        InputBinding firstAvailableDefault = FindFirstDefaultPathAvailable(newBinding, binding, secondDupBinding);
                                        if (firstAvailableDefault != newBinding)
                                        {
                                            Debug.Log("CheckDuplicateBindings: the first available default path is: " + firstAvailableDefault.path + " for the action: " + firstAvailableDefault.action.ToString());
                                            dupAction.ApplyBindingOverride(dupBindingIndex, firstAvailableDefault.path);
                                            wasDuplicateChanged = true;
                                        }
                                    }
                                }
                            }
                        }

                        //dupAction.ApplyBindingOverride(dupBindingIndex, newBinding.path);
                        //CheckForDuplicatesForSwapping(dupAction, dupBindingIndex, newBinding.path);
                        //return true;
                        breakLoop = true;
                        break;
                    }
                    if (breakLoop)
                        break;
                }
            }

            /*foreach (InputBinding binding in action.actionMap.bindings)
            {   
                
            }*/

            // check for duplicate composite binding
            if (allCompositeParts)
            {
                for (int i = 1; i < bindingIndex; i++)
                {
                    if (action.bindings[i].effectivePath == newBinding.effectivePath)
                    {
                        //Debug.Log("Duplicate binding found: " + newBinding.effectivePath);
                        return true;
                    }
                }
            }
            return false;
        }
        bool CheckIfPathExistsInActions(InputAction actionToCheck, int indexToCheck, string pathToCheck)
        {
            Debug.Log("CheckIfPathExistsInActions: action to check: " + actionToCheck.ToString() + " index to check: " + indexToCheck + " path to check: " + pathToCheck);
            bool pathAlreadyExists = false;
            InputBinding bindingtoCheck = actionToCheck.bindings[indexToCheck];

            foreach (InputActionMap map in ResetAllBindings.instance.actionMapsToCheck)
            {
                foreach (InputBinding binding in map.bindings)
                {
                    if (binding.action == bindingtoCheck.action)
                    {
                        //oldBindingPath = binding.effectivePath;
                        continue;
                    }
                    if (binding.effectivePath == pathToCheck)
                    {
                        pathAlreadyExists = true;
                        break;
                    }
                }
                if (pathAlreadyExists)
                    break;
            }

            /*foreach (InputBinding binding in actionToCheck.actionMap.bindings)
            {
                
            }*/
            return pathAlreadyExists;
        }
        InputBinding CheckWhatActionIsUsingPath(InputBinding bindingToCheck, int indexToCheck, string pathToCheck)
        {
            InputBinding bindingToReturn = bindingToCheck;
            //InputBinding bindingtoCheck = actionToCheck.bindings[indexToCheck];
            InputAction actionToCheck = this.GetComponent<StoreControls>().PlayerControls.FindAction(bindingToCheck.action);

            foreach (InputActionMap map in ResetAllBindings.instance.actionMapsToCheck)
            {
                bool breakLoop = false;
                foreach (InputBinding binding in map.bindings)
                {
                    if (binding.action == bindingToCheck.action)
                    {
                        //oldBindingPath = binding.effectivePath;
                        continue;
                    }
                    if (binding.effectivePath == pathToCheck)
                    {
                        bindingToReturn = binding;
                        breakLoop = true;
                        break;
                    }
                }
                if (breakLoop)
                    break;
            }

            /*foreach (InputBinding binding in actionToCheck.actionMap.bindings)
            {
                
            }*/
            return bindingToReturn;
        }
        InputBinding FindFirstDefaultPathAvailable(InputBinding newBinding, InputBinding duplicateBinding, InputBinding secondDuplicateBinding)
        {
            InputBinding bindingToReturn = newBinding;

            InputAction actionToCheck = this.GetComponent<StoreControls>().PlayerControls.FindAction(duplicateBinding.action);

            foreach (InputActionMap map in ResetAllBindings.instance.actionMapsToCheck)
            {
                bool breakLoop = false;
                foreach (InputBinding binding in map.bindings)
                {
                    if (binding.action == newBinding.action || binding.action == duplicateBinding.action || binding.action == secondDuplicateBinding.action)
                    {
                        //oldBindingPath = binding.effectivePath;
                        continue;
                    }
                    if (!binding.hasOverrides)
                        continue;
                    /*if (binding.effectivePath == pathToCheck)
                    {
                        bindingToReturn = binding;
                        break;
                    }*/
                    if (binding.path.ToLower().Contains("leftstick") || binding.path.ToLower().Contains("rightstick"))
                    {
                        continue;
                    }
                    Debug.Log("FindFirstDefaultPathAvailable: checking for binding action: " + binding.action.ToString() + " and its path: " + binding.path + " effective path: " + binding.effectivePath + " override path: " + binding.overridePath + " is part of composite? " + binding.isPartOfComposite);
                    if (binding.isComposite || binding.isPartOfComposite)
                    {
                        InputAction compositeAction = this.GetComponent<StoreControls>().PlayerControls.FindAction(binding.action);
                        Debug.Log("FindFirstDefaultPathAvailable: binding is a composite. Total number of bindings: " + compositeAction.bindings.Count().ToString());
                        for (int i = 0; i < compositeAction.bindings.Count(); i++)
                        {
                            Debug.Log("FindFirstDefaultPathAvailable: compositive bindings for " + binding.action.ToString() + " : " + compositeAction.bindings[i].name + " : " + compositeAction.bindings[i].path + " and binding groups? " + compositeAction.bindings[i].groups);
                            if (compositeAction.bindings[i].groups == duplicateBinding.groups)
                            {
                                if (!CheckIfPathExistsInActions(compositeAction, i, compositeAction.bindings[i].path))
                                {
                                    Debug.Log("FindFirstDefaultPathAvailable: The first available binding whose default path is not used is for the action: " + binding.action.ToString() + " and the path will be: " + binding.path);
                                    bindingToReturn = binding;
                                    breakLoop = true;
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        Debug.Log("FindFirstDefaultPathAvailable: binding IS NOT A composite.");
                        InputAction actionForBinding = this.GetComponent<StoreControls>().PlayerControls.FindAction(binding.action);
                        int indexForBinding = actionForBinding.GetBindingIndex(binding.groups, binding.path);
                        if (!CheckIfPathExistsInActions(actionForBinding, indexForBinding, binding.path))
                        {
                            Debug.Log("FindFirstDefaultPathAvailable: The first available binding whose default path is not used is for the action: " + binding.action.ToString() + " and the path will be: " + binding.path);
                            bindingToReturn = binding;
                            breakLoop = true;
                            break;
                        }
                    }
                }
                if (breakLoop)
                    break;
            }

            /*foreach (InputBinding binding in actionToCheck.actionMap.bindings)
            {
                
                
            }*/

            return bindingToReturn;
        }
        bool CheckForDuplicatesForSwapping(InputAction actionToCheck, int indexToCheck, string newPathToCheck)
        {
            bool duplicate = false;
            InputBinding bindingtoCheck = actionToCheck.bindings[indexToCheck];
            foreach (InputBinding binding in actionToCheck.actionMap.bindings)
            {
                if (binding.action == bindingtoCheck.action)
                {
                    //oldBindingPath = binding.effectivePath;
                    continue;
                }
                if (binding.effectivePath == newPathToCheck)
                {
                    Debug.Log("CheckForDuplicatesForSwapping: Duplicate Binding with effective path as the pathToCheck. Duplicate Binding action: " + binding.action.ToString() + " duplicate effective path: " + binding.effectivePath + " binding path: " + binding.path);
                    InputAction dupAction = this.GetComponent<StoreControls>().PlayerControls.FindAction(binding.action);
                    int dupBindingIndex = dupAction.GetBindingIndex(binding.groups, binding.path);
                    dupAction.ApplyBindingOverride(dupBindingIndex, bindingtoCheck.path);
                }
            }
            
            return duplicate;
        }

        protected void OnEnable()
        {
            if (s_RebindActionUIs == null)
                s_RebindActionUIs = new List<RebindActionUI>();
            s_RebindActionUIs.Add(this);
            if (s_RebindActionUIs.Count == 1)
                InputSystem.onActionChange += OnActionChange;
        }

        protected void OnDisable()
        {
            m_RebindOperation?.Dispose();
            m_RebindOperation = null;

            s_RebindActionUIs.Remove(this);
            if (s_RebindActionUIs.Count == 0)
            {
                s_RebindActionUIs = null;
                InputSystem.onActionChange -= OnActionChange;
            }
        }

        // When the action system re-resolves bindings, we want to update our UI in response. While this will
        // also trigger from changes we made ourselves, it ensures that we react to changes made elsewhere. If
        // the user changes keyboard layout, for example, we will get a BoundControlsChanged notification and
        // will update our UI to reflect the current keyboard layout.
        private static void OnActionChange(object obj, InputActionChange change)
        {
            if (change != InputActionChange.BoundControlsChanged)
                return;

            var action = obj as InputAction;
            var actionMap = action?.actionMap ?? obj as InputActionMap;
            var actionAsset = actionMap?.asset ?? obj as InputActionAsset;

            for (var i = 0; i < s_RebindActionUIs.Count; ++i)
            {
                var component = s_RebindActionUIs[i];
                var referencedAction = component.actionReference?.action;
                if (referencedAction == null)
                    continue;

                if (referencedAction == action ||
                    referencedAction.actionMap == actionMap ||
                    referencedAction.actionMap?.asset == actionAsset)
                    component.UpdateBindingDisplay();
            }
        }

        [Tooltip("Reference to action that is to be rebound from the UI.")]
        [SerializeField]
        private InputActionReference m_Action;

        [SerializeField]
        private string m_BindingId;

        [SerializeField]
        private InputBinding.DisplayStringOptions m_DisplayStringOptions;

        [Tooltip("Text label that will receive the name of the action. Optional. Set to None to have the "
            + "rebind UI not show a label for the action.")]
        [SerializeField]
        private TextMeshProUGUI m_ActionLabel;

        [Tooltip("Text label that will receive the current, formatted binding string.")]
        [SerializeField]
        private TextMeshProUGUI m_BindingText;

        [Tooltip("Optional UI that will be shown while a rebind is in progress.")]
        [SerializeField]
        private GameObject m_RebindOverlay;

        [Tooltip("Optional text label that will be updated with prompt for user input.")]
        [SerializeField]
        private TextMeshProUGUI m_RebindText;

        [Tooltip("Event that is triggered when the way the binding is display should be updated. This allows displaying "
            + "bindings in custom ways, e.g. using images instead of text.")]
        [SerializeField]
        private UpdateBindingUIEvent m_UpdateBindingUIEvent;

        [Tooltip("Event that is triggered when an interactive rebind is being initiated. This can be used, for example, "
            + "to implement custom UI behavior while a rebind is in progress. It can also be used to further "
            + "customize the rebind.")]
        [SerializeField]
        private InteractiveRebindEvent m_RebindStartEvent;

        [Tooltip("Event that is triggered when an interactive rebind is complete or has been aborted.")]
        [SerializeField]
        private InteractiveRebindEvent m_RebindStopEvent;

        private InputActionRebindingExtensions.RebindingOperation m_RebindOperation;

        private static List<RebindActionUI> s_RebindActionUIs;

        // We want the label for the action name to update in edit mode, too, so
        // we kick that off from here.
        #if UNITY_EDITOR
        protected void OnValidate()
        {
            UpdateActionLabel();
            UpdateBindingDisplay();
        }
        
        #endif
        private void Start()
        {
            UpdateActionLabel();
            UpdateBindingDisplay();
        }
        private void UpdateActionLabel()
        {
            if (m_ActionLabel != null)
            {
                var action = m_Action?.action;
                m_ActionLabel.text = action != null ? action.name : string.Empty;
            }
        }

        [Serializable]
        public class UpdateBindingUIEvent : UnityEvent<RebindActionUI, string, string, string>
        {
        }

        [Serializable]
        public class InteractiveRebindEvent : UnityEvent<RebindActionUI, InputActionRebindingExtensions.RebindingOperation>
        {
        }
    }
}
