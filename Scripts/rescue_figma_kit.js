// Adapted variable-collection and variant helpers from the installed Figma skill.
const made=[];const mark=n=>(made.push(n.id),n);
const page=await figma.getNodeByIdAsync('0:1');await figma.setCurrentPageAsync(page);page.name='01 Workday foundations';
const existing=page.findOne(n=>n.name==='BAU Rescue V2 review');if(existing)return {existing:existing.id};
const fonts={display:{family:'Bricolage Grotesque',style:'96pt ExtraBold'},label:{family:'Bricolage Grotesque',style:'Bold'},reader:{family:'Atkinson Hyperlegible',style:'Regular'},bold:{family:'Atkinson Hyperlegible',style:'Bold'},dela:{family:'Dela Gothic One',style:'Regular'},fredoka:{family:'Fredoka',style:'Bold'}};
for(const f of Object.values(fonts))await figma.loadFontAsync(f);
async function createVariableCollection(name,modeNames){const collection=figma.variables.createVariableCollection(name);const modeIds={};collection.renameMode(collection.modes[0].modeId,modeNames[0]);modeIds[modeNames[0]]=collection.modes[0].modeId;for(let i=1;i<modeNames.length;i++)modeIds[modeNames[i]]=collection.addMode(modeNames[i]);return {collection,modeIds};}
const {collection,modeIds}=await createVariableCollection('BAU / Workday',['Daylight']);const mode=modeIds.Daylight;const tokens={};
const hex=s=>({r:parseInt(s.slice(0,2),16)/255,g:parseInt(s.slice(2,4),16)/255,b:parseInt(s.slice(4,6),16)/255});
for(const [name,value] of Object.entries({ink:'203F40',paper:'F7EBCD',white:'FFF9E9',dark:'152E2D',mustard:'E7B647',coral:'D36F55',green:'528774',rule:'D6C5A1'})){
 const p=figma.variables.createVariable('primitive/'+name,collection,'COLOR');p.scopes=[];p.setValueForMode(mode,hex(value));p.setVariableCodeSyntax('WEB','var(--bau-'+name+'-raw)');
 const v=figma.variables.createVariable('color/'+name,collection,'COLOR');v.scopes=['FRAME_FILL','SHAPE_FILL','TEXT_FILL','STROKE_COLOR'];v.setValueForMode(mode,{type:'VARIABLE_ALIAS',id:p.id});v.setVariableCodeSyntax('WEB','var(--bau-'+name+')');tokens[name]=v;
}
for(const size of [4,8,12,16,24,32,48]){const v=figma.variables.createVariable('space/'+size,collection,'FLOAT');v.scopes=['GAP'];v.setValueForMode(mode,size);v.setVariableCodeSyntax('WEB','var(--bau-space-'+size+')');tokens['gap'+size]=v;}
const rad=figma.variables.createVariable('radius/paper',collection,'FLOAT');rad.scopes=['CORNER_RADIUS'];rad.setValueForMode(mode,8);rad.setVariableCodeSyntax('WEB','var(--bau-radius-paper)');tokens.radius=rad;
function paint(name){return figma.variables.setBoundVariableForPaint({type:'SOLID',color:hex(name==='dark'?'152E2D':'F7EBCD')},'color',tokens[name]);}
const styles={};for(const [name,size,font] of [['Display',40,'display'],['Title',32,'display'],['Section',24,'label'],['Body',18,'reader'],['Prompt',20,'bold'],['Small',14,'reader']]){const s=figma.createTextStyle();s.name='BAU/'+name;s.fontName=fonts[font];s.fontSize=size;s.lineHeight={unit:'PERCENT',value:130};styles[name]=s;}
function text(parent,value,style='Body',color='ink',width=500){const t=mark(figma.createText());t.fontName=styles[style].fontName;t.textStyleId=styles[style].id;t.characters=value;t.fills=[paint(color)];t.textAutoResize='HEIGHT';t.resize(width,Math.ceil(styles[style].fontSize*1.3));parent.appendChild(t);return t;}
function column(name,w,h,color='paper'){const f=mark(figma.createAutoLayout('VERTICAL'));f.name=name;f.resize(w,h);f.primaryAxisSizingMode='FIXED';f.counterAxisSizingMode='FIXED';f.paddingTop=f.paddingBottom=f.paddingLeft=f.paddingRight=24;f.itemSpacing=16;f.setBoundVariable('itemSpacing',tokens.gap16);f.setBoundVariable('paddingLeft',tokens.gap24);f.setBoundVariable('paddingRight',tokens.gap24);f.fills=[paint(color)];f.cornerRadius=8;f.setBoundVariable('cornerRadius',tokens.radius);return f;}
function row(parent,w,h,gap=12){const f=mark(figma.createAutoLayout('HORIZONTAL'));f.resize(w,h);f.primaryAxisSizingMode='FIXED';f.counterAxisSizingMode='FIXED';f.itemSpacing=gap;f.setBoundVariable('itemSpacing',tokens['gap'+gap]);f.fills=[];parent.appendChild(f);return f;}
function icon(parent,name,size){const f=mark(figma.createNodeFromSvg(ICONS[name]));f.name='Illustration/'+name;f.resize(size,size);parent.appendChild(f);for(const c of f.findAll())made.push(c.id);return f;}
const review=column('BAU Rescue V2 review',2040,1410,'dark');review.x=80;review.y=80;review.paddingTop=36;review.paddingLeft=review.paddingRight=36;review.itemSpacing=24;page.appendChild(review);
text(review,'BUSINESS AS USUAL  /  WORKDAY EDITION','Display','paper',1920);
text(review,'One action. One object. One readable result.','Body','paper',1900);
const comps=[];
for(const [name,w,h,value] of [['Button',230,50,'OPEN HOOD'],['Input glyph',42,42,'E'],['Attached tab',118,42,'JOB'],['Status stamp',140,44,'FILED']]){
 const variants=[];for(const [i,state] of ['Ready','Focus','Pressed','Disabled'].entries()){
  const c=mark(figma.createComponent());c.name='State='+state;c.resize(w,h);c.layoutMode='HORIZONTAL';c.primaryAxisAlignItems='CENTER';c.counterAxisAlignItems='CENTER';c.primaryAxisSizingMode='FIXED';c.counterAxisSizingMode='FIXED';c.cornerRadius=6;c.setBoundVariable('cornerRadius',tokens.radius);c.fills=[paint(state==='Pressed'?'green':state==='Disabled'?'rule':state==='Focus'?'mustard':'white')];c.strokes=[paint('ink')];c.strokeWeight=state==='Focus'?3:1;c.paddingLeft=c.paddingRight=8;c.setBoundVariable('paddingLeft',tokens.gap8);c.setBoundVariable('paddingRight',tokens.gap8);page.appendChild(c);
  const label=text(c,value,'Prompt',state==='Pressed'?'white':'ink',w-16);label.textAutoResize='WIDTH_AND_HEIGHT';variants.push(c);
 }
 const set=mark(figma.combineAsVariants(variants,page));set.name='BAU/'+name;set.description='Shared workday '+name.toLowerCase()+'. Focus uses shape and color; pressed state responds immediately.';set.x=2220;set.y=80+comps.length*260;set.children.forEach((v,i)=>{v.x=i*(w+16);v.y=0;});set.resize((w+16)*4,Math.max(h+16,90));comps.push(set);
}
const conceptRow=row(review,1968,600,24);const conceptIds=[];
for(const [i,name] of ['Clipboard / work slip','Field notebook','Folded instruction sheet'].entries()){
 const c=column(name,640,590);conceptRow.appendChild(c);conceptIds.push(c.id);
 text(c,name,'Section');text(c,'MECHANIC  /  VAN 08','Small');text(c,'Battery replacement','Title');
 const tabs=row(c,592,42,8);for(const tab of ['JOB','ITEMS','RECORDS','TOWN']){const inst=mark(comps[2].children[tab==='JOB'?1:0].createInstance());tabs.appendChild(inst);inst.findAllWithCriteria({types:['TEXT']})[0].characters=tab;}
 const steps=row(c,592,i===2?104:90,16);for(const item of ['hood','test','replace','sign']){const sc=column('Step/'+item,132,90,'white');sc.paddingTop=sc.paddingBottom=sc.paddingLeft=sc.paddingRight=4;sc.itemSpacing=2;steps.appendChild(sc);icon(sc,item,48);text(sc,({hood:'OPEN',test:'TEST',replace:'REPLACE',sign:'SIGN'})[item],'Small','ink',124);}
 const action=row(c,592,180,24);icon(action,'hood',164);const info=column('Current action',394,180,'white');info.paddingTop=12;info.paddingBottom=8;action.appendChild(info);text(info,'01  Open Hood','Section','ink',344);text(info,'Lift the marked latch.','Body','ink',344);const use=mark(comps[0].children[0].createInstance());info.appendChild(use);
 if(i===1){const side=row(c,592,24,16);text(side,'SERVICE TICKET','Small','ink',240);text(side,'BAY 1  •  2/5','Small','ink',240);}
 else text(c,'Bay 1 / Van 08  ·  2/5 steps','Body','ink',592);
 text(c,i===0?'Selected: one focal surface, no empty page.':i===1?'Compact, but page turns delay comparison.':'Good induction card; weaker for live records.','Small','ink',592);
}
text(review,'TYPE IN USE  /  IDENTICAL REPAIR CONTENT','Section','paper',1900);
const typeRow=row(review,1968,388,24);const typeIds=[];
for(const [i,key] of ['display','dela','fredoka'].entries()){
 const c=column('Typography '+String.fromCharCode(65+i),640,374);typeRow.appendChild(c);typeIds.push(c.id);
 text(c,(['A  Bricolage + Atkinson','B  Dela + Atkinson','C  Fredoka + Atkinson'])[i],'Small');const heading=text(c,'Battery replacement','Title','ink',592);heading.fontName=fonts[key];heading.fontSize=32;
 const r=row(c,592,94,16);icon(r,'battery',92);const t=column('Reader sample',484,94,'white');t.paddingTop=t.paddingBottom=4;t.paddingLeft=t.paddingRight=8;t.itemSpacing=4;r.appendChild(t);text(t,'E  Open Hood','Prompt','ink',462);text(t,'Bay 1 / Van 08 · 2/5','Body','ink',462);
 text(c,'Open  →  Check  →  Replace  →  Verify  →  Sign','Body','ink',592);text(c,'$19.90  ·  07:48   /   O0 1Il B8 5689','Body','ink',592);
 text(c,i===0?'Recommended: expressive heading, calm reader.':i===1?'Wide packaging forms; keep titles short.':'Soft and friendly; less workplace character.','Small','ink',592);
}
return {createdNodeIds:made,reviewId:review.id,conceptIds,typeIds,componentIds:comps.map(c=>c.id),collectionId:collection.id,variables:await figma.variables.getLocalVariablesAsync().then(v=>v.map(x=>({id:x.id,name:x.name,scopes:x.scopes}))),textStyles:Object.values(styles).map(s=>({id:s.id,name:s.name})),pageId:page.id};
