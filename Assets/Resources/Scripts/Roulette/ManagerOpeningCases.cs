﻿using System.Collections.Generic;
using System.Linq;
using Resources.Scripts.Enums;
using Resources.Scripts.Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Resources.Scripts.Roulette
{
    public class ManagerOpeningCases : MonoBehaviour
    {
        [Header("Prefabs")] [SerializeField] private GameObject itemPrefab;
        [SerializeField] private GameObject mainPanel;

        [Header("Texts")] [SerializeField] private TextMeshProUGUI nameCase;
        [SerializeField] private TextMeshProUGUI startScrollButtonText;

        [Header("Win panel elements")] [SerializeField]
        private GameObject winnerPanel;

        [SerializeField] private WinElement winnerItemPrefab;

        [Header("Buttons")] [SerializeField] private Button startScrollButton;

        [Header("Settings")] [SerializeField] private float scrollSpeed;
        [SerializeField] private float startPositionPanelLeft;
        [SerializeField] private float spacingBetweenElements;

        private Image startScrollButtonImage;
        private RectTransform m_RectTransformPanel;
        private List<GameObject> listPrefabs;
        private List<IItem> items;
        private float speed;
        private bool canScroll;
        private float widthPrefab;
        private int priceCase;
        private TypePrice priceTypeCase;

        private void Start()
        {
            startScrollButtonImage = startScrollButton.GetComponent<Image>();
            widthPrefab = itemPrefab.GetComponent<ListElement>().Width();
            m_RectTransformPanel = mainPanel.GetComponent<RectTransform>();
            listPrefabs = new List<GameObject>();
        }

        private void FixedUpdate()
        {
            if (!canScroll) return;
            speed = Mathf.MoveTowards(speed, 0, Random.Range(190, 250) * Time.deltaTime);
            m_RectTransformPanel.offsetMin -= new Vector2(speed, 0) * Time.deltaTime;
            if (speed <= 0)
            {
                canScroll = false;
                ShowPanelWinItem(listPrefabs[GetWinItemIndex()]);
            }
        }

        public void StartScroll()
        {
            if (!canScroll)
            {
                if (priceTypeCase == TypePrice.Gem)
                {
                    User.AddGem(-priceCase);
                }
                else
                {
                    User.AddMoney(-priceCase);
                }

                canScroll = true;
                startScrollButton.enabled = false;
                ManagerEvent.ActivateChangeMoney();
            }
        }

        private int GetWinItemIndex()
        {
            var finishPositionPanelLeft = m_RectTransformPanel.offsetMin.x;
            var rangeScrollLine = (finishPositionPanelLeft - startPositionPanelLeft) * (-1);
            var result = (int)((rangeScrollLine + 3 * widthPrefab + 2 * spacingBetweenElements +
                                spacingBetweenElements / 2) /
                               (widthPrefab + spacingBetweenElements));
            return result;
        }

        private void ShowPanelWinItem(GameObject winItem)
        {
            winnerPanel.SetActive(true);
            var winElement = winItem.GetComponent<ListElement>();
            winnerItemPrefab.SetTitle(winElement.GetName());
            winnerItemPrefab.SetMainImage(winElement.GetMainImage().sprite);
            winnerItemPrefab.SetBackgroundImage(winElement.GetBackgroundImage().sprite);
            var item = new Item(
                winElement.GetName(),
                winElement.GetMainImage().sprite,
                winElement.GetBackgroundImage().sprite,
                winElement.GetTypePriceImage().sprite,
                winElement.GetTypePrice(),
                winElement.GetPrice());
            User.AddItem(item);
        }

        public void ClosePanelWinner()
        {
            canScroll = false;
            winnerPanel.SetActive(false);
            GenerateRandomItem();
            startScrollButton.enabled = !(User.GetCountMoney() < priceCase);
        }

        public void ExtraFinishScroll()
        {
            canScroll = false;
        }

        private void GenerateScrollLine()
        {
            if (listPrefabs.Count != 0) return;

            for (var i = 0; i < 56; i++)
            {
                var instantiate = Instantiate(itemPrefab, mainPanel.transform);
                listPrefabs.Add(instantiate);
            }
        }

        private void GenerateRandomItem()
        {
            speed = scrollSpeed;
            m_RectTransformPanel.offsetMin = new Vector2(startPositionPanelLeft, 20);
            foreach (var elementMeta in listPrefabs.Select(prefab => prefab.GetComponent<ListElement>()))
            {
                elementMeta.SetMainImage(items[Random.Range(0, items.Count)].GetMainImage());
                elementMeta.SetPriceImage(items[Random.Range(0, items.Count)].GetMainImage());
                elementMeta.SetBackgroundImage(items[Random.Range(0, items.Count)].GetBackgroundImage());
                elementMeta.SetTitle(items[Random.Range(0, items.Count)].GetName());
                elementMeta.SetPrice(TypePrice.Gem, items[Random.Range(0, items.Count)].GetPrice());
            }
        }

        public void ClickOnCase(string nameCase, int priceCase, TypePrice typePrice, List<IItem> items)
        {
            this.priceCase = priceCase;
            this.nameCase.text = nameCase;
            priceTypeCase = typePrice;
            startScrollButtonText.text = "Открыть (" + this.priceCase + ")";
            this.items = items;
            startScrollButton.enabled = !(User.GetCountMoney() < priceCase);
            GenerateScrollLine();
            GenerateRandomItem();
        }

        public void ClearPrefabs()
        {
            if (listPrefabs == null || listPrefabs.Count == 0) return;

            foreach (var prefab in listPrefabs)
            {
                Destroy(prefab);
            }

            listPrefabs.Clear();
        }
    }
}