// Editable native-game UI mockups using the existing BAU kit, never flattened screenshots.
const ids=[];const mark=n=>(ids.push(n.id),n);
let page=figma.root.children.find(p=>p.name==='02 Player screens');if(!page){page=figma.createPage();page.name='02 Player screens';}
await figma.setCurrentPageAsync(page);
if(page.children.some(n=>n.name==='01 Main menu'))return {existing:page.id};
const styles=Object.fromEntries((await figma.getLocalTextStylesAsync()).filter(s=>s.name.startsWith('BAU/')).map(s=>[s.name.slice(4),s]));
for(const s of Object.values(styles))await figma.loadFontAsync(s.fontName);
const variables=Object.fromEntries((await figma.variables.getLocalVariablesAsync()).map(v=>[v.name,v]));
const paint=k=>figma.variables.setBoundVariableForPaint({type:'SOLID',color:{r:0,g:0,b:0}},'color',variables['color/'+k]);
function gap(node,key,value){node[key]=value;const v=variables['space/'+value];if(v)node.setBoundVariable(key,v);}
function text(parent,value,style='Body',width=560,color='ink'){
 const t=mark(figma.createText());t.fontName=styles[style].fontName;t.textStyleId=styles[style].id;t.characters=value;t.fills=[paint(color)];t.textAutoResize='HEIGHT';t.resize(width,styles[style].fontSize*1.3);parent.appendChild(t);return t;
}
function stack(name,w,h,color='paper',padding=24){const n=mark(figma.createAutoLayout('VERTICAL'));n.name=name;n.resize(w,h);n.primaryAxisSizingMode='FIXED';n.counterAxisSizingMode='FIXED';gap(n,'itemSpacing',16);for(const k of ['paddingTop','paddingBottom','paddingLeft','paddingRight'])gap(n,k,padding);n.fills=[paint(color)];n.cornerRadius=8;n.setBoundVariable('cornerRadius',variables['radius/paper']);return n;}
function row(parent,w,h,g=12){const n=mark(figma.createAutoLayout('HORIZONTAL'));n.name='Related controls';n.resize(w,h);n.primaryAxisSizingMode='FIXED';n.counterAxisSizingMode='FIXED';gap(n,'itemSpacing',g);n.fills=[];parent.appendChild(n);return n;}
const kit={};for(const [key,id] of Object.entries({button:'2:44',glyph:'2:53',tab:'2:62',stamp:'2:71'}))kit[key]=await figma.getNodeByIdAsync(id);
function control(parent,kind,label,state='Ready',width){const main=kit[kind].children.find(c=>c.name==='State='+state);const n=mark(main.createInstance());parent.appendChild(n);const property=Object.keys(n.componentProperties).find(k=>n.componentProperties[k].type==='TEXT');n.setProperties({[property]:label});if(width)n.resize(width,n.height);return n;}
const illustrations={};let iy=0;
for(const [name,svg] of Object.entries(ICONS)){
 const c=mark(figma.createComponent());c.name='BAU/Illustration/'+name;c.resize(256,256);c.x=5560+(iy%3)*280;c.y=80+Math.floor(iy/3)*280;c.description='Original BAU editable pictogram; shared geometric source with Unity SVG and PNG.';page.appendChild(c);const v=mark(figma.createNodeFromSvg(svg));c.appendChild(v);for(const d of v.findAll())ids.push(d.id);illustrations[name]=c;iy++;
}
function icon(parent,name,size=86){const n=mark(illustrations[name].createInstance());n.rescale(size/256);parent.appendChild(n);return n;}
const surface=mark(figma.createComponent());surface.name='BAU/Paper surface';surface.layoutMode='VERTICAL';surface.resize(640,514);surface.primaryAxisSizingMode='FIXED';surface.counterAxisSizingMode='FIXED';surface.fills=[paint('paper')];surface.cornerRadius=8;surface.setBoundVariable('cornerRadius',variables['radius/paper']);for(const k of ['paddingTop','paddingBottom','paddingLeft','paddingRight'])gap(surface,k,24);gap(surface,'itemSpacing',16);surface.x=6500;surface.y=80;surface.description='Single focal work slip, no decorative empty page. Unity uses the same paper/radius/spacing tokens.';page.appendChild(surface);
const screens=[];function screen(name){const n=mark(figma.createFrame());n.name=name;n.resize(1280,720);n.fills=[paint('dark')];n.x=80+(screens.length%4)*1344;n.y=80+Math.floor(screens.length/4)*784;page.appendChild(n);screens.push(n);return n;}
function card(screen,name,w=640,h=514){const c=stack(name,w,h);gap(c,'itemSpacing',12);screen.appendChild(c);c.x=(1280-w)/2;c.y=(720-h)/2;return c;}
function heading(c,over,title){text(c,over,'Small',c.width-48);text(c,title,'Title',c.width-48);}
function tabs(c,selected='JOB'){const r=row(c,c.width-48,42,8);for(const label of ['JOB','ITEMS','RECORDS','TOWN'])control(r,'tab',label,label===selected?'Focus':'Ready',Math.floor((c.width-72)/4));}
function steps(c,labels,images,done=0){const r=row(c,c.width-48,91,12);for(let i=0;i<5;i++){const step=stack('Work stage / '+labels[i],108,91,i<done?'green':'white',4);r.appendChild(step);step.itemSpacing=2;icon(step,i<done?'check':images[i],49);const t=text(step,labels[i],'Small',100,i<done?'white':'ink');t.textAlignHorizontal='CENTER';}}
function action(c,image,title,hint){const r=row(c,c.width-48,174,24);icon(r,image,142);const copy=stack('Current object and action',c.width-214,174,'white',12);r.appendChild(copy);copy.itemSpacing=8;text(copy,title,'Section',copy.width-24);text(copy,hint,'Body',copy.width-24);control(copy,'glyph','E');}
function diagram(parent,w,h){const n=mark(figma.createFrame());n.name='Editable town map';n.resize(w,h);n.fills=[paint('rule')];parent.appendChild(n);const road=mark(figma.createRectangle());road.resize(w,Math.round(h*.15));road.y=h*.50;road.fills=[paint('dark')];n.appendChild(road);for(const [name,x,y,ww,hh] of [['MARKET',.08,.10,.24,.32],['SQUARE',.36,.12,.21,.28],['GARAGE',.66,.10,.26,.30],['YARD',.65,.72,.26,.20]]){const b=mark(figma.createRectangle());b.resize(w*ww,h*hh);b.x=w*x;b.y=h*y;b.fills=[paint('paper')];n.appendChild(b);const t=text(n,name,'Small',w*ww);t.x=b.x+4;t.y=b.y+4;}for(const [name,x,y,color] of [['You',.45,.42,'green'],['Next',.71,.32,'mustard']]){const p=mark(figma.createEllipse());p.name=name;p.resize(12,12);p.x=w*x;p.y=h*y;p.fills=[paint(color)];n.appendChild(p);}return n;}
function hud(s,title='Open hood'){const c=stack('Compact work HUD',326,90,'paper',12);s.appendChild(c);c.x=24;c.y=24;c.itemSpacing=4;text(c,'MECHANIC','Small',302);text(c,title,'Prompt',302);const bars=row(c,302,4,8);for(let i=0;i<5;i++){const b=mark(figma.createRectangle());b.resize(52,4);b.fills=[paint(i<2?'green':'rule')];bars.appendChild(b);}const map=stack('Live minimap',152,154,'paper',8);s.appendChild(map);map.x=1104;map.y=24;map.itemSpacing=4;text(map,'DAYLIGHT','Small',136);diagram(map,136,110);}
function prompt(s,label,key='E',hint=''){const c=stack('Contextual input',352,62,'paper',8);s.appendChild(c);c.x=464;c.y=630;c.itemSpacing=0;const r=row(c,336,46,8);control(r,'glyph',key,'Ready',40);const copy=stack('Verb and optional input',288,46,'paper',0);copy.itemSpacing=2;r.appendChild(copy);text(copy,label,'Prompt',288);if(hint)text(copy,hint,'Small',288);}
{
 const s=screen('01 Main menu');const c=card(s,'Connection desk',580,474);heading(c,'DAYLIGHT / WORKDAY EDITION','Business as Usual');text(c,'Ordinary jobs, extraordinary suspicion.','Body',532);control(c,'button','START SOLO','Focus',532);const r=row(c,532,50);control(r,'button','HOST','Ready',260);control(r,'button','JOIN','Ready',260);text(c,'One player. Clearly labelled town services.','Small',532);control(c,'button','SETTINGS','Ready',532);
}
{
 const s=screen('02 Join a workday');const c=card(s,'Host address',540,335);heading(c,'STAFF CONNECTION','Join the town');text(c,'Host address','Prompt',492);const field=stack('IPv4 entry',492,43,'white',8);c.appendChild(field);text(field,'127.0.0.1','Prompt',476);const r=row(c,492,50);control(r,'button','JOIN','Focus',240);control(r,'button','BACK','Ready',240);
}
{
 const s=screen('03 Staff desk');const c=card(s,'Choose your work',720,540);heading(c,'DAYLIGHT / STAFF DESK','Choose your work');const r=row(c,672,270,12);for(const [title,image,verbs] of [['Shopkeeper','shop','Stock / serve / supply'],['Mechanic','wrench','Inspect / repair / verify'],['Sanitation','cart','Collect / sort / recover']]){const p=stack(title,216,270,'white',12);r.appendChild(p);p.itemSpacing=8;icon(p,image,102);text(p,title,'Section',192);text(p,verbs,'Small',192);control(p,'button','START','Ready',192);}text(c,'Solo services keep unattended jobs moving.','Body',672);control(c,'button','BACK','Ready',672);
}
{
 const s=screen('04 Illustrated controls');const c=card(s,'First minute',640,424);heading(c,'START HERE','At your fingertips');const r=row(c,592,174,24);for(const [name,image,key] of [['Move','map','WASD'],['Look','mouse','MOUSE'],['Use','check','E']]){const p=stack(name,180,174,'white',8);r.appendChild(p);p.itemSpacing=4;icon(p,image,86);text(p,name,'Section',164);text(p,key,'Prompt',164);}text(c,'Aim at a real object. Follow the small work cue.','Body',592);control(c,'button','NEXT','Focus',592);
}
{
 const s=screen('05 Compact HUD');hud(s);prompt(s,'Open hood');
}
{
 const s=screen('06 Wrench interaction');hud(s,'Loosen battery clamp');prompt(s,'Drag to loosen clamp','LMB','DRAG RIGHT / Q CANCEL');const c=stack('Accepted result',310,42,'paper',8);s.appendChild(c);c.x=24;c.y=638;text(c,'Clamp released','Prompt',294);
}
{
 const s=screen('07 Illustrated handbook');const c=card(s,'One work slip');heading(c,'DAYLIGHT / MECHANIC COPY',"Today's work");tabs(c);steps(c,['Open','Check','Replace','Verify','Sign'],['hood','test','replace','test','sign'],2);action(c,'battery','Lift old battery','The clamp is released. Lift the handle.');const r=row(c,592,32);text(r,'One load / in your hands','Small',450);control(r,'button','WHY?','Ready',130);
}
{
 const s=screen('08 Inventory inspection');const c=card(s,'Physical inventory');heading(c,'DAYLIGHT / YOUR WORK SLIP','In your hands');tabs(c,'ITEMS');const r=row(c,592,222,24);icon(r,'parcel',166);const info=stack('One physical parcel',400,222,'white',12);r.appendChild(info);text(info,'Garage parcel','Section',376);text(info,'PACK-8A-001','Prompt',376);text(info,'One battery and its packaging.','Body',376);text(info,'Sealed / contents unchecked','Small',376);text(c,'Q returns this same parcel safely.','Body',592);
}
{
 const s=screen('09 Customer order');const c=card(s,'Retail order');heading(c,'DAYLIGHT / SHOPKEEPER COPY','Serve a customer');tabs(c);steps(c,['Receive','Stock','Serve','Dispatch','File'],['parcel','shelf','checkout','parcel','receipt'],2);action(c,'cola','Scan customer order','One Sunbeam Cola / $1.80');text(c,'Place goods, then scan. Earlier handlers are omitted.','Small',592);
}
{
 const s=screen('10 Service ticket');const c=card(s,'Garage service ticket');heading(c,'DAYLIGHT / MECHANIC COPY','Verify the repair');tabs(c);steps(c,['Open','Check','Replace','Verify','Sign'],['hood','test','replace','test','sign'],3);action(c,'test','Test repair','Connect the two battery posts.');text(c,'12.6 V after fitting / sign after hood closure.','Small',592);
}
{
 const s=screen('11 Collection route');const c=card(s,'Sanitation route');heading(c,'DAYLIGHT / SANITATION COPY','Collect and recover');tabs(c);steps(c,['Collect','Inspect','Sort','File','Finish'],['cart','bag','sort','receipt','check'],1);action(c,'cart','Take cart to yard','Load several batches, or carry one directly.');text(c,'Aim at a visible batch to choose what to lift.','Small',592);
}
{
 const s=screen('12 Partial records');const c=card(s,'Profession record');heading(c,'DAYLIGHT / YOUR FILE','Processed work');tabs(c,'RECORDS');const r=row(c,592,100,24);icon(r,'receipt',79);const title=stack('Record heading',489,100,'white',8);r.appendChild(title);text(title,'Service ticket','Section',473);text(title,'PACK-8A-001','Prompt',473);text(c,'Battery fitted and tested; old battery sent to mixed treatment.','Body',592);text(c,'Earlier handlers / not recorded','Small',592);control(c,'stamp','FILED','Pressed',180);const nav=row(c,592,42);control(nav,'button','PREVIOUS','Ready',290);control(nav,'button','NEXT','Ready',290);
}
{
 const s=screen('13 Expanded map');const c=card(s,'Town map',690,570);heading(c,'DAYLIGHT / TOWN MAP','Where your work goes');diagram(c,642,370);text(c,'YOU  Â·  NEXT STOP       M Close','Small',642);
}
{
 const s=screen('14 Pause');const c=card(s,'Staff break',450,435);heading(c,'STAFF BREAK','Take a breather');text(c,'Your work is safe here.','Body',402);control(c,'button','RESUME','Focus',402);const r=row(c,402,50);control(r,'button','SETTINGS','Ready',195);control(r,'button','CONTROLS','Ready',195);control(c,'button','LEAVE WORKDAY','Ready',402);
}
{
 const s=screen('15 Settings');const c=card(s,'Preferences',580,512);heading(c,'DAYLIGHT / PREFERENCES','Your preferences');for(const label of ['Volume  80%','Look sensitivity  24%','Field of view  74']){text(c,label,'Prompt',532);const rail=mark(figma.createRectangle());rail.resize(532,9);rail.fills=[paint('rule')];c.appendChild(rail);}control(c,'button','REDUCED MOTION  OFF','Ready',532);control(c,'button','WINDOWED  1280 Ã— 720','Ready',532);control(c,'button','BACK','Ready',532);
}
{
 const s=screen('16 Shift report');const c=card(s,'End of shift',620,482);heading(c,'DAYLIGHT / END OF SHIFT','Shift filed');icon(c,'check',172);text(c,'Mechanic shift','Section',572);text(c,'Town supply  15 / 20','Prompt',572);const r=row(c,572,50);control(r,'button','NEW WORKDAY','Focus',280);control(r,'button','VIEW RECORDS','Ready',280);
}
return {pageId:page.id,createdNodeIds:ids,screenIds:screens.map(s=>({id:s.id,name:s.name,width:s.width,height:s.height})),illustrationComponentIds:Object.values(illustrations).map(c=>c.id),surfaceId:surface.id,instanceCount:page.findAllWithCriteria({types:['INSTANCE']}).length,textCount:page.findAllWithCriteria({types:['TEXT']}).length};


