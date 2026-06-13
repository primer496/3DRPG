using System.ComponentModel;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using InventorySystem.ViewModel;
using InventorySystem.Model;
using InventorySystem.Utils;
using TaskManager;

public class InventoryUIController : MonoBehaviour
{
    [Header("ViewModel Reference")]
    public InventoryViewModel viewModel;

    private VisualElement root;
    private InventoryBindableData bindableData;

    private Button backButton;
    private Button resetButton;
    private Button displayButton;

    private Button[] categoryTabs = new Button[6];
    private Button sortByRarity;
    private Button sortByRecent;
    private Button uiSwitchButton;

    private VisualElement[] itemSlots = new VisualElement[42];
    private Label[] itemCounts = new Label[42];

    private Button deleteItemButton;
    private Button sortButton;
    private Button useItemButton;

    private VisualElement itemPreviewPanel;
    private Label previewTitle;
    private Label previewDescription;

    private bool isDeleteMode = false;
    private HashSet<int> itemsToDelete = new HashSet<int>();

    private void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;

        backButton = root.Q<Button>("BackButton");
        resetButton = root.Q<Button>("ResetButton");
        displayButton = root.Q<Button>("DisplayButton");

        for (int i = 0; i < 6; i++)
            categoryTabs[i] = root.Q<Button>($"CategoryTab{i + 1}");

        sortByRarity = root.Q<Button>("SortByRarity");
        sortByRecent = root.Q<Button>("SortByRecent");
        uiSwitchButton = root.Q<Button>("UISwitchButton");

        for (int i = 0; i < 42; i++)
        {
            itemSlots[i] = root.Q<VisualElement>($"ItemSlot{i}");
            itemCounts[i] = root.Q<Label>($"ItemCount{i}");
        }

        deleteItemButton = root.Q<Button>("DeleteItemButton");
        sortButton = root.Q<Button>("SortButton");
        useItemButton = root.Q<Button>("UseItemButton");

        itemPreviewPanel = root.Q<VisualElement>("ItemPreviewPanel");
        previewTitle = root.Q<Label>("PreviewTitle");
        previewDescription = root.Q<Label>("PreviewDescription");

        RegisterEvents();
    }

    private void Start()
    {
        if (viewModel != null)
        {
            bindableData = viewModel.bindableData;
            if (bindableData != null)
            {
                // 鎴戜滑浣跨敤鏇村畨鍏ㄣ€侀€昏緫鍙帶鐨� C# 浜嬩欢椹卞姩鏈哄埗锛屾棤闇€ UXML 寮曟搸灞傞潰鐨� dataSource 缁戝畾
                SubscribeBindableChanges();
            }
        }

        // 鍒濆闅愯棌锛歎I Toolkit 鐢� display:none 浠ｆ浛 SetActive(false)
        root.style.display = DisplayStyle.None;
    }

    private void RegisterEvents()
    {
        backButton.clicked += OnBackButtonClicked;
        resetButton.clicked += OnResetButtonClicked;
        displayButton.clicked += OnDisplayButtonClicked;

        for (int i = 0; i < 6; i++)
        {
            int idx = i + 1;
            categoryTabs[i].clicked += () => OnCategoryTabClicked(idx);
        }

        sortByRarity.clicked += OnSortByRarityClicked;
        sortByRecent.clicked += OnSortByRecentClicked;
        uiSwitchButton.clicked += OnUISwitchButtonClicked;

        deleteItemButton.clicked += OnDeleteItemButtonClicked;
        sortButton.clicked += OnSortButtonClicked;
        useItemButton.clicked += OnUseItemButtonClicked;

        // 涓烘瘡涓墿鍝佹Ы娣诲姞鐐瑰嚮浜嬩欢
        for (int i = 0; i < 42; i++)
        {
            if (itemSlots[i] != null)
            {
                int index = i;
                itemSlots[i].RegisterCallback<ClickEvent>(evt => {
                    OnItemSlotClicked(index);
                    // 闃绘浜嬩欢鍐掓场锛岄伩鍏嶈Е鍙戞牴瀹瑰櫒鐨勭偣鍑讳簨浠�
                    evt.StopPropagation();
                });
            }
        }

        // 涓洪瑙堥潰鏉挎坊鍔犵偣鍑讳簨浠讹紝闃绘浜嬩欢鍐掓场
        if (itemPreviewPanel != null)
        {
            itemPreviewPanel.RegisterCallback<ClickEvent>(evt => {
                evt.StopPropagation();
            });
        }

        // 涓烘牴瀹瑰櫒娣诲姞鐐瑰嚮浜嬩欢锛岀偣鍑诲叾浠栧尯鍩熷叧闂瑙�
        root.RegisterCallback<ClickEvent>(evt => {
            if (bindableData != null && bindableData.isPreviewVisible)
            {
                if (viewModel != null)
                {
                    viewModel.SelectItem(-1);
                }
            }
        });
    }

    private void SubscribeBindableChanges()
    {
        if (bindableData == null) return;

        // 2. 瀵逛簬鎺у埗鏍峰紡鐨勩€佹棤娉曡缁勪欢鍘熺敓鐩存帴瀹圭撼鐨勬帶鍒堕噺锛岄噰鐢–#浜嬩欢鐩戝惉
        bindableData.OnCategoryChanged += RefreshUI;
        bindableData.OnPreviewStateChanged += UpdatePreviewPanel;
        bindableData.OnTabChanged += () =>
        {
            UpdateSortTabHighlight(bindableData.activeSortTab);
            UpdateCategoryTabHighlight(bindableData.activeCategoryTab);
        };

        RefreshUI();
    }

    private void UnsubscribeBindableChanges()
    {
        if (bindableData != null)
        {
            bindableData.OnCategoryChanged -= RefreshUI;
            bindableData.OnPreviewStateChanged -= UpdatePreviewPanel;
            // 鍖垮悕 delegate 灏嗗湪 OnDisable 涓殢瀵硅薄閿€姣佽€屽洖鏀�
        }
    }

    private void OnEnable()
    {
        if (viewModel != null)
        {
            viewModel.inventoryModel.OnInventoryChanged += RefreshUI;
            RefreshUI();
        }

        EventBus.Instance.Subscribe("ToggleInventory", ToggleInventory);
        EventBus.Instance.Subscribe("CloseInventory", CloseInventory);
    }

    private void OnDisable()
    {
        if (viewModel != null && viewModel.inventoryModel != null)
            viewModel.inventoryModel.OnInventoryChanged -= RefreshUI;

        UnsubscribeBindableChanges();

        EventBus.Instance.Unsubscribe("ToggleInventory", ToggleInventory);
        EventBus.Instance.Unsubscribe("CloseInventory", CloseInventory);
    }

    /// <summary>由 QuestUIController 互斥关闭背包时调用，不触发再次锁/解锁</summary>
    private void CloseInventory()
    {
        if (root != null && root.style.display != DisplayStyle.None)
            root.style.display = DisplayStyle.None;
    }

    private void RefreshUI()
    {
        if (viewModel == null) return;

        for (int i = 0; i < 42; i++)
        {
            string countText = viewModel.GetItemCountText(i);
            itemCounts[i].text = countText;

            string iconPath = viewModel.GetIconPath(i);
            if (!string.IsNullOrEmpty(iconPath))
            {
                Sprite icon = ItemIconLoader.LoadItemIcon(iconPath);
                itemSlots[i].style.backgroundImage = icon != null ? new StyleBackground(icon) : null;
            }
            else
            {
                itemSlots[i].style.backgroundImage = null;
            }

            // 鏍规嵁鏄惁閫変腑鍒犻櫎搴旂敤閬僵棰滆壊
            itemSlots[i].style.unityBackgroundImageTintColor = (isDeleteMode && itemsToDelete.Contains(i)) ? new Color(1f, 0.3f, 0.3f) : Color.white;
        }
    }

    private void UpdatePreviewPanel()
    {
        if (bindableData == null) return;

        // 鏇存柊UI鏂囨湰娓叉煋
        previewTitle.text = bindableData.previewTitle;
        previewDescription.text = bindableData.previewDescription;

        // 鏍规嵁鐘舵€佺粦瀹� USS class 鏍峰紡锛屼粠鑰屾帶鍒舵樉闅愬拰瑙嗚鏁堟灉
        if (bindableData.isPreviewVisible)
        {
            itemPreviewPanel.AddToClassList("visible");
        }
        else
        {
            itemPreviewPanel.RemoveFromClassList("visible");
        }
    }

    private void UpdateSortTabHighlight(int tabIndex)
    {
        sortByRarity.RemoveFromClassList("active-inventory-tab");
        sortByRecent.RemoveFromClassList("active-inventory-tab");

        if (tabIndex == 0)
            sortByRarity.AddToClassList("active-inventory-tab");
        else if (tabIndex == 1)
            sortByRecent.AddToClassList("active-inventory-tab");
    }

    private void UpdateCategoryTabHighlight(int categoryIndex)
    {
        for (int i = 0; i < 6; i++)
            categoryTabs[i].RemoveFromClassList("active-tab");

        int tabIdx = categoryIndex - 1;
        if (tabIdx >= 0 && tabIdx < 6)
            categoryTabs[tabIdx].AddToClassList("active-tab");
    }

    private void CancelDeleteMode()
    {
        if (!isDeleteMode) return;
        isDeleteMode = false;
        if (deleteItemButton != null)
            deleteItemButton.style.backgroundColor = new StyleColor(StyleKeyword.Null); // 鎭㈠鍒濆鏍峰紡
        itemsToDelete.Clear();
        RefreshUI(); // 鍙栨秷鍙樼孩閬僵
    }

    private void OnBackButtonClicked() 
    {
        CancelDeleteMode();
        if (root != null)
        {
            root.style.display = DisplayStyle.None;
            EventBus.Instance.RaiseInputLock(false);
        }
    }

    /// <summary>
    /// 澶栭儴璋冪敤姝ゆ柟娉曞垏鎹㈣儗鍖呯姸鎬侊紙鎵撳紑鎴栧叧闂級
    /// </summary>
    public void ToggleInventory()
    {
        if (root != null)
        {
            if (root.style.display == DisplayStyle.None)
            {
                // 互斥：先关闭任务面板
                EventBus.Instance.Raise("CloseQuestLog");
                root.style.display = DisplayStyle.Flex;
                EventBus.Instance.RaiseInputLock(true);
                RefreshUI();
            }
            else
            {
                root.style.display = DisplayStyle.None;
                EventBus.Instance.RaiseInputLock(false);
            }
        }
    }

    private void OnResetButtonClicked() 
    {
        CancelDeleteMode();
        Debug.Log("鎵ц鑳屽寘閲嶇疆");
        if (viewModel != null)
            viewModel.ResetInventory();
    }

    private void OnDisplayButtonClicked() 
    {
        Debug.Log("鏄剧ず璁惧畾 - 姝ゅ鍙互鍔犲叆鎺у埗鐗╁搧妯″瀷闅愯棌/鏄剧ず鐨勯€昏緫");
        // 濡傛灉鏈夐澶栫殑闈㈡澘锛屽彲浠ュ湪姝ゅ Toggle
    }

    private void OnCategoryTabClicked(int categoryIndex)
    {
        CancelDeleteMode();
        if (viewModel != null)
            viewModel.ChangeCategory(categoryIndex);
    }

    private void OnSortByRarityClicked()
    {
        CancelDeleteMode();
        if (viewModel != null)
            viewModel.SetActiveSortTab(0);
    }

    private void OnSortByRecentClicked()
    {
        CancelDeleteMode();
        if (viewModel != null)
            viewModel.SetActiveSortTab(1);
    }

    private void OnUISwitchButtonClicked() 
    {
        Debug.Log("鐣岄潰璁剧疆琚偣鍑� - 鍛煎嚭娓告垙绯荤粺鑿滃崟");
    }

    private void OnDeleteItemButtonClicked()
    {
        if (!isDeleteMode)
        {
            // 绗竴娆＄偣鍑伙細杩涘叆鍒犻櫎妯″紡
            isDeleteMode = true;
            deleteItemButton.style.backgroundColor = new StyleColor(new Color(0.8f, 0.2f, 0.2f, 1f)); // 鎸夐挳鍙樼孩楂樹寒
            itemsToDelete.Clear();
        }
        else
        {
            // 绗簩娆＄偣鍑伙細鎵ц鐪熷疄鍒犻櫎骞堕€€鍑哄垹闄ゆā寮�
            if (viewModel != null && itemsToDelete.Count > 0)
            {
                viewModel.DeleteItems(itemsToDelete);
            }
            CancelDeleteMode();
        }
    }

    private void OnSortButtonClicked() 
    {
        CancelDeleteMode();
        if (viewModel != null)
            viewModel.SortInventory();
    }

    private void OnUseItemButtonClicked()
    {
        CancelDeleteMode();
        if (viewModel != null && bindableData.selectedSlotIndex >= 0)
        {
            viewModel.UseItem(bindableData.selectedSlotIndex);
        }
    }

    // 绉婚櫎鎮仠鐩稿叧鏂规硶锛屽洜涓轰笉鍐嶄娇鐢�


    private void OnItemSlotClicked(int index)
    {
        if (isDeleteMode)
        {
            // 鍒犻櫎妯″紡锛氳礋璐ｉ€変腑涓庡弽閫夊彉绾�
            if (viewModel == null) return;
            var slot = viewModel.GetSlotAt(index);
            if (slot.IsEmpty) return; // 涓嶉€夋嫨绌烘牸瀛�

            if (itemsToDelete.Contains(index))
            {
                itemsToDelete.Remove(index);
                itemSlots[index].style.unityBackgroundImageTintColor = Color.white; // 鎭㈠鍘熸牱
            }
            else
            {
                itemsToDelete.Add(index);
                itemSlots[index].style.unityBackgroundImageTintColor = new Color(1f, 0.3f, 0.3f); // 鍥剧墖鍙樼孩
            }
        }
        else
        {
            // 姝ｅ父妯″紡锛氳礋璐ｆ甯稿睍绀虹墿鍝�
            if (viewModel != null)
                viewModel.SelectItem(index);
        }
    }
}
