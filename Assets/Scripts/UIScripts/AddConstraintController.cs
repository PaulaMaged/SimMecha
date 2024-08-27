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
        private Dictionary<((int robotId, string link), (int robotId, string link)), string> _linkConstraints;
        private static List<string> constraintStrings = new List<string>();

        private void Awake()
        {
            _toggleButton.onClick.AddListener(OnToggleButtonClick);
            _confirmButton.onClick.AddListener(OnConfirmButtonClick);
        }

        void Start()
        {
            _linkConstraints = new Dictionary<((int robotId, string link), (int robotId, string link)), string>();

            robotDropdowns = _robotDropdowns.GetComponentsInChildren<TMP_Dropdown>();
            linkDropdowns = _linkDropdowns.GetComponentsInChildren<TMP_Dropdown>();
            constraintDropdown = _constraintDropdowns.GetComponent<TMP_Dropdown>();

            _constraintPanel.SetActive(false);

            List<string> constraintOptions = new List<string>
            {
                "Constraint 1",
                "Constraint 2",
                "Constraint 3"
            };

            constraintDropdown.ClearOptions();
            constraintDropdown.AddOptions(constraintOptions);
            
            PopulateConstraintPanel();
        }

        private void PopulateConstraintPanel()
        {
            robots.Clear();
            selectedLinksByRobot.Clear();

            int counter = 0;
            
            robots.Add(new TMP_Dropdown.OptionData("plane"));
            
            foreach (GameObject robot in RuntimeURDFLoader.ImportedRobots)
            {
                robots.Add(new TMP_Dropdown.OptionData(robot.name));
                selectedLinksByRobot[robots.Count - 1] = new List<string>();
                
                links.Clear();

                if(robot.name.Equals("plane")) links.Add(new TMP_Dropdown.OptionData("plane"));
                else AddLinksToLinkMenu(links, robot.transform);
                
                linkDropdowns[counter].options.Clear();
                linkDropdowns[counter].AddOptions(links);

                counter++;
            }

            foreach (var robotDropdown in robotDropdowns)
            {
                robotDropdown.options.Clear();
                robotDropdown.AddOptions(robots);
            }
            
            // Register selection events
            for (int i = 0; i < robotDropdowns.Length; i++)
            {
                int index = i; // Capture the index for the lambda
                robotDropdowns[i].onValueChanged.AddListener(delegate { OnRobotDropdownChanged(robotDropdowns[index], index); });
                linkDropdowns[i].onValueChanged.AddListener(delegate { OnLinkDropdownChanged(linkDropdowns[index], index); });
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

            if (firstRobotIndex == 0)
            {
                // Add to the _linkConstraints dictionary
                _linkConstraints.Add(
                    ((-1, "plane"), 
                        (RuntimeURDFLoader.NewImportedRobots[secondRobotIndex].RobotId, RuntimeURDFLoader.NewImportedRobots[secondRobotIndex].Links[secondLinkIndex])),
                    constraint
                );
            } else if (secondRobotIndex == 0)
            {
                // Add to the _linkConstraints dictionary
                _linkConstraints.Add(
                    ((RuntimeURDFLoader.NewImportedRobots[secondRobotIndex].RobotId, RuntimeURDFLoader.NewImportedRobots[secondRobotIndex].Links[secondLinkIndex]),
                        (-1, "plane")),
                    constraint
                );
            }
            else
            {
                // Add to the _linkConstraints dictionary
                _linkConstraints.Add(
                    ((RuntimeURDFLoader.NewImportedRobots[firstRobotIndex].RobotId, RuntimeURDFLoader.NewImportedRobots[firstRobotIndex].Links[firstLinkIndex]), 
                        (RuntimeURDFLoader.NewImportedRobots[secondRobotIndex].RobotId, RuntimeURDFLoader.NewImportedRobots[secondRobotIndex].Links[secondLinkIndex])),
                    constraint
                );
            }

            Debug.Log("Constraint added successfully!\n" +
                      $"Id1: { RuntimeURDFLoader.NewImportedRobots[firstRobotIndex].RobotId } -- Link1: { RuntimeURDFLoader.NewImportedRobots[firstRobotIndex].Links[firstLinkIndex] }" +
                      $"Id2: { RuntimeURDFLoader.NewImportedRobots[secondRobotIndex].RobotId } -- Link1: { RuntimeURDFLoader.NewImportedRobots[secondRobotIndex].Links[secondLinkIndex] }" +
                      $"Constraint: { constraint }"
                );
        }

        public void PopulateConstraintStringsList()
        {
            foreach (var keyValuePair in _linkConstraints)
            {
                constraintStrings.Add($"{keyValuePair.Key.Item1.robotId}, {keyValuePair.Key.Item1.link}, {keyValuePair.Key.Item2.robotId}, {keyValuePair.Key.Item2.link}, {keyValuePair.Value}");
            }
        }

        public List<string> GetConstraintStrings() {  return constraintStrings; }
    }
}
