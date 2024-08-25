using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Robotics.UrdfImporter;
using UnityEngine;
using UnityEngine.UI;

namespace UIScripts
{
    public class AddConstraintController : MonoBehaviour
    {
        [SerializeField] private Button _toggleButton;
        [SerializeField] private GameObject _constraintPanel;
        [SerializeField] private GameObject _robotDropdowns;
        [SerializeField] private GameObject _linkDropdowns;
        [SerializeField] private GameObject _constraintDropdowns;
        [SerializeField] private Button _confirmButton;

        private TMP_Dropdown[] robotDropdowns;
        private TMP_Dropdown[] linkDropdowns;
        private TMP_Dropdown constraintDropdown;

        private List<TMP_Dropdown.OptionData> robots = new List<TMP_Dropdown.OptionData>();
        private List<TMP_Dropdown.OptionData> links = new List<TMP_Dropdown.OptionData>();

        private Dictionary<int, List<string>> selectedLinksByRobot = new Dictionary<int, List<string>>();
        // Dictionary<(Robot1.Link2, Robot2.Link3), constraint>;
        private Dictionary<((int robotId, int linkNumber), (int robotId, int linkNumber)), string> _linkConstraints;

        private void Awake()
        {
            _toggleButton.onClick.AddListener(OnToggleButtonClick);
            _toggleButton.onClick.AddListener(OnConfirmButtonClick);
        }

        void Start()
        {
            _linkConstraints = new Dictionary<((int robotId, int linkNumber), (int robotId, int linkNumber)), string>();

            robotDropdowns = _robotDropdowns.GetComponentsInChildren<TMP_Dropdown>();
            linkDropdowns = _linkDropdowns.GetComponentsInChildren<TMP_Dropdown>();
            constraintDropdown = _constraintDropdowns.GetComponent<TMP_Dropdown>();

            _constraintPanel.SetActive(false);

            // Register selection events
            for (int i = 0; i < robotDropdowns.Length; i++)
            {
                int index = i; // Capture the index for the lambda
                robotDropdowns[i].onValueChanged.AddListener(delegate { OnRobotDropdownChanged(robotDropdowns[index], index); });
                linkDropdowns[i].onValueChanged.AddListener(delegate { OnLinkDropdownChanged(linkDropdowns[index], index); });
            }

            List<string> constraintOptions = new List<string>
            {
                "Constraint 1",
                "Constraint 2",
                "Constraint 3"
            };

            constraintDropdown.AddOptions(constraintOptions);
        }

        private void PopulateConstraintPanel()
        {
            robots.Clear();
            links.Clear();
            selectedLinksByRobot.Clear();

            foreach (GameObject robot in RuntimeURDFLoader.ImportedRobots)
            {
                robots.Add(new TMP_Dropdown.OptionData(robot.name));
                selectedLinksByRobot[robots.Count - 1] = new List<string>();

                foreach (Transform link in robot.transform)
                {
                    AddLinksToLinkMenu(links, link);
                }
            }

            foreach (var dropdown in robotDropdowns)
            {
                dropdown.options = robots;
            }

            foreach (var dropdown in linkDropdowns)
            {
                dropdown.options = links;
            }
        }

        private static void AddLinksToLinkMenu(List<TMP_Dropdown.OptionData> linkDropdownOptions, Transform parent)
        {
            if (parent.GetComponent<UrdfLink>() != null)
            {
                linkDropdownOptions.Add(new TMP_Dropdown.OptionData(parent.name));
            }

            foreach (Transform child in parent)
            {
                AddLinksToLinkMenu(linkDropdownOptions, child);
            }
        }

        private void OnRobotDropdownChanged(TMP_Dropdown robotDropdown, int dropdownIndex)
        {
            int selectedRobotIndex = robotDropdown.value;

            // Filter out links that have already been selected for this robot
            var availableLinks = links.Where(link => !selectedLinksByRobot[selectedRobotIndex].Contains(link.text)).ToList();

            // Update the corresponding link dropdown with available links
            linkDropdowns[dropdownIndex].ClearOptions();
            linkDropdowns[dropdownIndex].AddOptions(availableLinks);
        }

        private void OnLinkDropdownChanged(TMP_Dropdown linkDropdown, int dropdownIndex)
        {
            int selectedRobotIndex = robotDropdowns[dropdownIndex].value;
            string selectedLinkName = linkDropdown.options[linkDropdown.value].text;

            if (!selectedLinksByRobot[selectedRobotIndex].Contains(selectedLinkName))
            {
                selectedLinksByRobot[selectedRobotIndex].Add(selectedLinkName);
            }
            else
            {
                // Handle duplicate link selection (optional: show error, revert selection)
                Debug.LogWarning("This link has already been selected for the chosen robot.");
            }
        }

        private void OnToggleButtonClick()
        {
            if (!_constraintPanel.activeSelf) PopulateConstraintPanel();
            _constraintPanel.SetActive(!_constraintPanel.activeSelf);
        }

        private void OnConfirmButtonClick()
        {
            // Gather selections from dropdowns
            int firstRobotIndex = robotDropdowns[0].value;
            int secondRobotIndex = robotDropdowns[1].value;

            int firstLinkIndex = linkDropdowns[0].value;
            int secondLinkIndex = linkDropdowns[1].value;

            string constraint = constraintDropdown.options[constraintDropdown.value].text;

            // Add to the _linkConstraints dictionary
            _linkConstraints.Add(
                ((firstRobotIndex, firstLinkIndex), (secondRobotIndex, secondLinkIndex)),
                constraint
            );

            Debug.Log("Constraint added successfully!");
        }
    }
}
