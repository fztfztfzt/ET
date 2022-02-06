using AssetBundles;
using System;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

namespace ET
{
	[ObjectSystem]
	public class UILoginComponentAwakeSystem : AwakeSystem<UILoginComponent>
	{
		public override void Awake(UILoginComponent self)
		{
			ReferenceCollector rc = self.GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();
			self.loginBtn = rc.Get<GameObject>("LoginBtn");
			
			self.loginBtn.GetComponent<Button>().onClick.AddListener(()=> { self.OnLogin(); });
			self.account = rc.Get<GameObject>("Account");
			self.password = rc.Get<GameObject>("Password");
			self.icon = rc.Get<GameObject>("Icon").GetComponent<Image>();
			//var sprites = Resources.LoadAll<Sprite>("cards/cards");
			SetSprite(self).Coroutine();
		}

		async ETTask SetSprite(UILoginComponent self)
        {
			var sprite = await ResourcesComponent.Instance.LoadAssetAtPathAsync<Sprite>("Assets/Res/images/1024Portraits/blue/attack/all_for_one.png");
			self.icon.sprite = sprite;
		}
	}
	
	public static class UILoginComponentSystem
	{
		public static void OnLogin(this UILoginComponent self)
		{
			LoginHelper.Login(
				self.DomainScene(), 
				ConstValue.LoginAddress, 
				self.account.GetComponent<InputField>().text, 
				self.password.GetComponent<InputField>().text).Coroutine();
		}
	}
}
