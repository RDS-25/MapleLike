using System;
using System.Collections.Generic;
using System.IO;
using Google.Protobuf.Protocol;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Profiling;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ItemManager : MonoBehaviour
{
    private ProfilerMarker  marker = new ProfilerMarker("TestCode"); // ProfilerMarker 사용
    
    

    public static ItemManager Instance { get; private set; }

    public ItemDatabase database;
    public List<Item> ItemList = new List<Item>();

    // ✅ 아이템 로드 완료 이벤트 선언
    public event Action OnItemsLoaded;
    
    AsyncOperationHandle handle;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        
        // LoadItems();
    
        
        
        LoadItemIcons();

    }
    //스크립터블 오브젝트를 만들어서 직접 드래그해서 넣는게 메모리에 좀더 맞지 않을까 ?
    //고정값은 json으로 하지말기
    /*03-22 스크립터블 오브젝트를 스크립트로 실행 할때 마다 만들고 Json 파싱해서 넣어 주면 메모리적으로 실행 해야되는게 많아서
    그냥 스크립터블 오브젝트를 직접 만들어서 데이터를 넣어주고 리스트에 넣어주자 -- 리펙토링 자문 사항 
     */
    

    async void LoadItemIcons()
    {
        foreach (var item in ItemList)
        {
            if (item.iconReference != null && item.iconReference.RuntimeKeyIsValid()) // ✅ 유효한 키인지 확인
            {
                try
                {
                    var handle = Addressables.LoadAssetAsync<Sprite>(item.iconReference);
                    await handle.Task; // ✅ 비동기 로드

                    if (handle.Status == AsyncOperationStatus.Succeeded)
                    {
                        item.IconSprite = handle.Result; // ✅ 아이콘 로드 완료
                        Debug.Log($"✔ {item.itemName} 아이콘 로드 완료!");
                    }
                    else
                    {
                        Debug.LogError($"❌ {item.itemName} 아이콘 로드 실패! ({item.iconReference})");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"⚠ {item.itemName} 아이콘 로드 중 예외 발생: {ex.Message}");
                }
            }
            else
            {
                Debug.LogWarning($"⚠ {item.itemName}의 아이콘 참조가 유효하지 않음!");
            }
        }
    }

    void LoadItems()
    {
        string path = Application.streamingAssetsPath + "/items.json";

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            //한번 읽어오기
            database = JsonUtility.FromJson<ItemDatabase>(json);

            // 아이템 정보 추가 후 아이콘 로딩을 비동기적으로 실행
            foreach (var itemData in database.items)
            {
                Item newItem = null;

                if (itemData.itemCategory == "Consumable")
                {
                    newItem = ScriptableObject.CreateInstance<Consumable>();
                    (newItem as Consumable).healAmount = itemData.healAmount;
                    (newItem as Consumable).MpAmount = itemData.MpAmount;
                }
                else if (itemData.itemCategory == "Equipment")
                {
                    newItem = ScriptableObject.CreateInstance<Equipment>();
                    Equipment newEquipment = newItem as Equipment;
                    newEquipment.attackPower = itemData.attackPower;
                    newEquipment.magicPower = itemData.magicPower;
                    newEquipment.defensePower = itemData.defensePower;
                    newEquipment.limitLevel = itemData.limitLevel;
                    newEquipment.classType = (ClassType)Enum.Parse(typeof(ClassType), itemData.classType);
                    newEquipment.parts = (Equipment.Parts)Enum.Parse(typeof(Equipment.Parts), itemData.parts);
                }

                if (newItem != null)
                {
                    newItem.id = itemData.id;
                    newItem.itemName = itemData.itemName;
                    newItem.description = itemData.description;
                    newItem.buyprice = itemData.buyprice;
                    newItem.sellprice = itemData.buyprice/3;
                    newItem.iconAddress = itemData.iconAddress;
                    newItem.ItemType =  (ItemType)Enum.Parse(typeof(ItemType),itemData.itemType);
                        
                      
                    ItemList.Add(newItem);

                    LoadItemIcon(newItem); // 아이콘 로드 시작
                }
            }
;
        }
        else
        {
            Debug.LogError("JSON 파일을 찾을 수 없습니다!");
        }
       
    }

    /// <summary>
    /// 어드레서블을 사용해서 구독을 이용한 갱신을 해야되어서 만든 함수
    /// </summary>
    /// <param name="item"> Item</param>
    void LoadItemIcon(Item item)
    {
        Addressables.LoadAssetAsync<Sprite>(item.iconAddress).Completed += (AsyncOperationHandle<Sprite> handle) =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                item.IconSprite = handle.Result;
                // Debug.Log($"{item.itemName} 아이콘 로드 성공: {item.iconAddress}");

                // 아이콘 로드 완료 후, UI 업데이트
                OnItemsLoaded?.Invoke(); // UI 업데이트 신호 전송
            }
            else
            {
                Debug.LogError($"{item.itemName} 아이콘 로드 실패: {item.iconAddress}");
            }
        };
    }
    
    /// <summary>
    ///  OnItemsLoaded 이벤트를 수동으로 실행할 수 있도록 함수 제공
    /// </summary>
    // 수동 사용 이유로 버튼으로 사고 팔고 한뒤 갱신이 바로 안되어서 강제로 넣었음
    public void TriggerOnItemsLoaded()
    {
        if (OnItemsLoaded != null)
        {
            OnItemsLoaded.Invoke();
        }
        else
        {
            Debug.LogWarning("⚠ OnItemsLoaded 이벤트를 구독한 리스너가 없습니다.");
        }
    }

}
