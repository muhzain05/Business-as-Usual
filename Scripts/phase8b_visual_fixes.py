from pathlib import Path
r=Path('Unity/BusinessAsUsual/Assets/Phase8B/Scripts')
p=r/'RescueInterface.Phase8B.cs';s=p.read_text(encoding='utf-8-sig');s=s.replace('destinationArrow.pivot=Mid;','destinationArrow.pivot=Mid;destinationArrow.anchoredPosition+=new Vector2(12.5f,-12.5f);');s=s.replace('owners[i]==NetworkShop.Nobody?"Unstaffed \u00b7 town service"','owners[i]==NetworkShop.Nobody?(i==3?"Optional helping hand":"Unstaffed \u00b7 town service")');p.write_text(s,encoding='utf-8-sig')
p=r/'SuspiciousDayService.cs';s=p.read_text(encoding='utf-8-sig').replace('new Vector3(0,.73f,-.245f)','new Vector3(0,.83f,-.265f)');p.write_text(s,encoding='utf-8-sig')
