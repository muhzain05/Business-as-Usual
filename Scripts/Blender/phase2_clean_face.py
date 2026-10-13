"""Clean only duplicated head texture pixels; preserve source images and cheek colour."""
import bpy, numpy as np, json
from pathlib import Path
ROOT=Path('D:/Business as Usual'); OUT=ROOT/'Docs/Diagnostics/Phase2/v002'
TEX=ROOT/'Assets/Characters/Sunny/Working/Textures/v002'; TEX.mkdir(parents=True,exist_ok=True)
obj=bpy.data.objects['Sunny_v002_Head']; source=obj.data.materials[0]
mat=source.copy(); mat.name='Sunny_v002_HeadClean'; obj.data.materials[0]=mat
nodes=[n for n in mat.node_tree.nodes if n.type=='TEX_IMAGE' and n.image]
color=next(n for n in nodes if n.image.colorspace_settings.name=='sRGB')
normalnode=next(n for n in mat.node_tree.nodes if n.type=='NORMAL_MAP')
normal=next((l.from_node for l in normalnode.inputs['Color'].links if l.from_node.type=='TEX_IMAGE'),None)
W,H=color.image.size; pixels=np.array(color.image.pixels[:],dtype=np.float32).reshape(H,W,4)
region=np.zeros((H,W),dtype=bool); uv=obj.data.uv_layers.active
obj.data.calc_loop_triangles()
for tri in obj.data.loop_triangles:
    pts=[obj.data.vertices[i].co for i in tri.vertices]; c=sum(pts,pts[0]*0)/3
    if not (c.y<-.12 and abs(c.x)<.15 and .86<c.z<1.035):continue
    q=np.array([(uv.data[i].uv.x*(W-1),uv.data[i].uv.y*(H-1)) for i in tri.loops])
    x0=max(0,int(np.floor(q[:,0].min())));x1=min(W-1,int(np.ceil(q[:,0].max())))
    y0=max(0,int(np.floor(q[:,1].min())));y1=min(H-1,int(np.ceil(q[:,1].max())))
    xx,yy=np.meshgrid(np.arange(x0,x1+1),np.arange(y0,y1+1))
    a,b,c=q; den=(b[1]-c[1])*(a[0]-c[0])+(c[0]-b[0])*(a[1]-c[1])
    if abs(den)<1e-6:continue
    u=((b[1]-c[1])*(xx-c[0])+(c[0]-b[0])*(yy-c[1]))/den
    v=((c[1]-a[1])*(xx-c[0])+(a[0]-c[0])*(yy-c[1]))/den
    region[y0:y1+1,x0:x1+1]|=(u>=-.01)&(v>=-.01)&(u+v<=1.01)
mask=region&(pixels[:,:,:3].mean(axis=2)<.42)
for _ in range(5):mask=mask|np.roll(mask,1,0)|np.roll(mask,-1,0)|np.roll(mask,1,1)|np.roll(mask,-1,1)
# Sample the nearby unpainted head, then smooth transitions deterministically.
valid=region&~mask&(pixels[:,:,:3].mean(axis=2)>.50)
fill=np.median(pixels[valid,:3],axis=0)
clean=pixels.copy();clean[mask,:3]=fill
for _ in range(32):
    average=(np.roll(clean,1,0)+np.roll(clean,-1,0)+np.roll(clean,1,1)+np.roll(clean,-1,1))/4
    clean[mask,:3]=average[mask,:3]
new=color.image.copy();new.name='Sunny_v002_HeadColor';new.pixels.foreach_set(clean.ravel());new.update()
new.filepath_raw=str(TEX/'Sunny_Head_Color_v002.png');new.file_format='PNG';new.save();new.pack();color.image=new
if normal:
    n=normal.image.copy();arr=np.array(n.pixels[:],dtype=np.float32).reshape(H,W,4)
    arr[mask,:3]=(.5,.5,1);n.pixels.foreach_set(arr.ravel());n.update()
    n.name='Sunny_v002_HeadNormal';n.filepath_raw=str(TEX/'Sunny_Head_Normal_v002.png');n.file_format='PNG';n.save();n.pack();normal.image=n
result={'pixels_cleaned':int(mask.sum()),'head_material':mat.name,'source_images_changed':False,'fill_srgb':fill.tolist(),
        'scope':'dark baked eyes/smile only, head-front UV region; original separate face geometry and cheeks retained'}
(OUT/'face-cleanup.json').write_text(json.dumps(result,indent=2));print(json.dumps(result))
