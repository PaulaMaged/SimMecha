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

        private List<TMP_Dropdown.OptionData> robots = new();
        private List<TMP_Dropdown.OptionData> links = new();

        private Dictionary<int, List<string>> selectedLinksByRobot = new();
        private Dictionary<((int robotId, string link), (int robotId, string link)), string> _linkConstraints;
        private static List<string> constraintStrings = new();

        private void Awake()
        {
            _toggleButton.onClick.AddListener(OnToggleButtonClick);
            _confirmButton.onClick.AddListener(OnConfirmButtonClick);
        }

        private void Start()
        {
            _linkConstraints = new Dictionary<((int robotId, string link), (int robotId, string link)), string>();

            robotDropdowns = _robotDropdowns.GetComponentsInChildren<TMP_Dropdown>();
            linkDropdowns = _linkDropdowns.GetComponentsInChildren<TMP_Dropdown>();
            constraintDropdown = _constraintDropdowns.GetComponent<TMP_Dropdown>();

            _constraintPanel.SetActive(false);

            var constraintOptions = new List<string>
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

            // Always add the "plane" option first
            robots.Add(new TMP_Dropdown.OptionData("plane"));
            selectedLinksByRobot.Add(0, new List<string> { "plane" });

            foreach (var linkDropdown in linkDropdowns)
            {
                linkDropdown.ClearOptions();
                linkDropdown.AddOptions(new List<string> { "plane" });
            }

            var robotIndex = 1;
            foreach (var robot in RuntimeURDFLoader.ImportedRobots)
            {
                robots.Add(new TMP_Dropdown.OptionData(robot.name));
                selectedLinksByRobot[robotIndex] = new List<string>();

                links.Clear();
                AddLinksToLinkMenu(links, robot.transform);

                linkDropdowns[robotIndex - 1].ClearOptions();
                linkDropdowns[robotIndex - 1].AddOptions(links.Select(link => link.text).ToList());

                robotIndex++;
            }

            foreach (var robotDropdown in robotDropdowns)
            {
                robotDropdown.options.Clear();
                robotDropdown.AddOptions(robots);
            }

            // Register selection events
            for (var i = 0; i < robotDropdowns.Length; i++)
            {
                var index = i; // Capture the index for the lambda
                robotDropdowns[i].onValueChanged.AddListener(delegate
                {
                    OnRobotDropdownChanged(robotDropdowns[index], index);
                });
                linkDropdowns[i].onValueChanged.AddListener(delegate
                {
                    OnLinkDropdownChanged(linkDropdowns[index], index);
                });
            }
        }

        private static void AddLinksToLinkMenu(List<TMP_Dropdown.OptionData> linkDropdownOptions, Transform parent)
        {
            if (parent.GetComponent<UrdfLink>() != null)
                linkDropdownOptions.Add(new TMP_Dropdown.OptionData(parent.name));

            foreach (Transform child in parent) AddLinksToLinkMenu(linkDropdownOptions, child);
        }

        private void OnRobotDropdownChanged(TMP_Dropdown robotDropdown, int dropdownIndex)
        {
            var selectedRobotIndex = robotDropdown.value;

            // Ensure there's a selected robot
            if (selectedRobotIndex >= 0 && selectedRobotIndex < robots.Count)
            {
                // Clear existing options in the link dropdown
                linkDropdowns[dropdownIndex].ClearOptions();

                if (selectedRobotIndex == 0)
                {
                    // If the first option (plane) is selected, only "plane" should be available
                    linkDropdowns[dropdownIndex].AddOptions(new List<string> { "plane" });
                }
                else
                {
                    // Populate the link dropdown with the available links for the selected robot
                    var availableLinks = links
                        .Where(link => !selectedLinksByRobot[selectedRobotIndex].Contains(link.text)).ToList();
                    linkDropdowns[dropdownIndex].AddOptions(availableLinks.Select(link => link.text).ToList());
                }
            }
        }

        private void OnLinkDropdownChanged(TMP_Dropdown linkDropdown, int dropdownIndex)
        {
            var selectedRobotIndex = robotDropdowns[dropdownIndex].value;
            var selectedLinkName = linkDropdown.options[linkDropdown.value].text;

            if (!selectedLinksByRobot[selectedRobotIndex].Contains(selectedLinkName))
                selectedLinksByRobot[selectedRobotIndex].Add(selectedLinkName);
        }

        private void OnToggleButtonClick()
        {
            if (!_constraintPanel.activeSelf) PopulateConstraintPanel();
            _constraintPanel.SetActive(!_constraintPanel.activeSelf);
        }

        private void OnConfirmButtonClick()
        {
            var firstRobotIndex = robotDropdowns[0].value - 1;
            var secondRobotIndex = robotDropdowns[1].value - 1;

            var firstLinkIndex = linkDropdowns[0].value;
            var secondLinkIndex = linkDropdowns[1].value;

            var constraint = constraintDropdown.options[constraintDropdown.value].text;

            // Validate indices before proceeding
            if (!IsValidIndex(firstRobotIndex, RuntimeURDFLoader.NewImportedRobots.Count) ||
                !IsValidIndex(secondRobotIndex, RuntimeURDFLoader.NewImportedRobots.Count))
            {
                PopUpController.Instance.ShowMessage("Invalid robot selection.");
                return;
            }

            if (firstRobotIndex == secondRobotIndex && firstLinkIndex == secondLinkIndex)
            {
                PopUpController.Instance.ShowMessage(
                    "Cannot process this request, since you cannot add a constraint between a link and itself");
                return;
            }

            if (!IsValidIndex(firstLinkIndex, RuntimeURDFLoader.NewImportedRobots[firstRobotIndex].Links.Count) ||
                !IsValidIndex(secondLinkIndex, RuntimeURDFLoader.NewImportedRobots[secondRobotIndex].Links.Count))
            {
                PopUpController.Instance.ShowMessage("Invalid link selection.");
                return;
            }

            if (firstRobotIndex == 0 || secondRobotIndex == 0)
                AddPlaneConstraint(firstRobotIndex, secondRobotIndex, firstLinkIndex, secondLinkIndex, constraint);
            else
                _linkConstraints.Add(
                    ((RuntimeURDFLoader.NewImportedRobots[firstRobotIndex].RobotId, RuntimeURDFLoader.NewImportedRobots[firstRobotIndex].Links[firstLinkIndex]),
                        (RuntimeURDFLoader.NewImportedRobots[secondRobotIndex].RobotId,
                            RuntimeURDFLoader.NewImportedRobots[secondRobotIndex].Links[secondLinkIndex])),
                    constraint
                );

            PopUpController.Instance.ShowMessage("Constraint added successfully!\n" +
                                                 $"Id1: {RuntimeURDFLoader.NewImportedRobots[firstRobotIndex].RobotId} -- Link1: {RuntimeURDFLoader.NewImportedRobots[firstRobotIndex].Links[firstLinkIndex]}" +
                                                 $"Id2: {RuntimeURDFLoader.NewImportedRobots[secondRobotIndex].RobotId} -- Link2: {RuntimeURDFLoader.NewImportedRobots[secondRobotIndex].Links[secondLinkIndex]}" +
                                                 $"Constraint: {constraint}"
            );
        }

        private void AddPlaneConstraint(int firstRobotIndex, int secondRobotIndex, int firstLinkIndex,
            int secondLinkIndex, string constraint)
        {
            if (!IsValidIndex(firstRobotIndex, RuntimeURDFLoader.NewImportedRobots.Count) ||
                !IsValidIndex(secondRobotIndex, RuntimeURDFLoader.NewImportedRobots.Count))
            {
                PopUpController.Instance.ShowMessage("Invalid robot or link selection.");
                return;
            }

            if (firstRobotIndex == 0)
                _linkConstraints.Add(
                    ((-1, "plane"),
                        (RuntimeURDFLoader.NewImportedRobots[secondRobotIndex].RobotId,
                            RuntimeURDFLoader.NewImportedRobots[secondRobotIndex].Links[secondLinkIndex])),
                    constraint
                );
            else if (secondRobotIndex == 0)
                _linkConstraints.Add(
                    ((RuntimeURDFLoader.NewImportedRobots[firstRobotIndex].RobotId, RuntimeURDFLoader.NewImportedRobots[firstRobotIndex].Links[firstLinkIndex]),
                        (-1, "plane")),
                    constraint
                );
        }

        private bool IsValidIndex(int index, int count)
        {
            Debug.Log("index: " + index);
            Debug.Log("count :" + count);
            return index >= 0 && index < count;
        }

        public void PopulateConstraintStringsList()
        {
            foreach (var keyValuePair in _linkConstraints)
                constraintStrings.Add(
                    $"{keyValuePair.Key.Item1.robotId}, {keyValuePair.Key.Item1.link}, {keyValuePair.Key.Item2.robotId}, {keyValuePair.Key.Item2.link}, {keyValuePair.Value}");
        }
    }
}