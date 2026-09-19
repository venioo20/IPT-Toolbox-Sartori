from __future__ import annotations
import os, sys, subprocess
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / ".video-tools"))
import imageio_ffmpeg

W, H, FPS, TOTAL = 1280, 720, 10, 120
OUT = ROOT / "docs" / "assets"
OUT.mkdir(parents=True, exist_ok=True)

FONT = Path(r"C:\Windows\Fonts\segoeui.ttf")
FONT_B = Path(r"C:\Windows\Fonts\segoeuib.ttf")
def f(size, bold=False): return ImageFont.truetype(str(FONT_B if bold else FONT), size)

INK=(16,24,40); BLUE=(21,94,239); SOFT=(242,244,247); MUTED=(102,112,133); WHITE=(255,255,255); GREEN=(7,148,85); LINE=(222,226,230); NAVY=(11,31,58); RED=(217,45,32)

def rr(d, box, radius=16, fill=WHITE, outline=None, width=1): d.rounded_rectangle(box, radius, fill=fill, outline=outline, width=width)
def text(d, xy, value, size=28, color=INK, bold=False, anchor=None): d.text(xy, value, font=f(size,bold), fill=color, anchor=anchor)
def wrap(d, xy, value, width, size=28, color=MUTED, bold=False, spacing=8):
    words=value.split(); lines=[]; line=""
    for word in words:
        trial=(line+" "+word).strip()
        if d.textlength(trial,font=f(size,bold))<=width: line=trial
        else: lines.append(line); line=word
    if line: lines.append(line)
    d.multiline_text(xy,"\n".join(lines),font=f(size,bold),fill=color,spacing=spacing)

def chrome(d, title="IPT TOOLBOX SARTORI", pro=False):
    rr(d,(70,54,1210,666),24,WHITE,LINE,2)
    d.rectangle((70,54,1210,128),fill=(250,251,252))
    rr(d,(94,76,138,120),10,BLUE); text(d,(116,98),"IPT",13,WHITE,True,"mm")
    text(d,(154,97),title,24,INK,True,"lm")
    if pro: rr(d,(1056,76,1182,116),20,(236,253,243)); text(d,(1119,96),"PRO ACTIF",14,GREEN,True,"mm")

def base_scene(kicker, title, subtitle):
    im=Image.new("RGB",(W,H),(247,249,252)); d=ImageDraw.Draw(im)
    text(d,(64,48),kicker,16,BLUE,True); wrap(d,(64,82),title,560,52,INK,True,10); wrap(d,(64,230),subtitle,530,23,MUTED,False,8)
    return im,d

def scene_intro():
    im,d=base_scene("DÉMONSTRATION • 2 MINUTES","Préparer un PC devient simple.","IPT Toolbox Sartori rassemble les logiciels, les profils et les licences professionnelles dans une seule interface Windows.")
    rr(d,(680,80,1180,620),25,WHITE,LINE,2); rr(d,(720,118,770,168),12,BLUE); text(d,(745,143),"IPT",14,WHITE,True,"mm"); text(d,(790,143),"TOOLBOX SARTORI",25,INK,True,"lm")
    for i,(n,c) in enumerate([("50 logiciels",BLUE),("8 profils",GREEN),("Simulation par défaut",NAVY)]): rr(d,(720,220+i*92,1140,284+i*92),12,SOFT); d.ellipse((742,241+i*92,764,263+i*92),fill=c); text(d,(784,252+i*92),n,21,INK,True,"lm")
    return im

def scene_profile():
    im=Image.new("RGB",(W,H),(239,244,255)); d=ImageDraw.Draw(im); chrome(d)
    text(d,(100,158),"1. Sélectionner un profil",30,INK,True); text(d,(100,198),"Choisissez une configuration prête à l’emploi.",19,MUTED)
    rr(d,(100,245,520,305),9,WHITE,LINE); text(d,(122,275),"Profil : Technicien",20,INK,True,"lm"); text(d,(482,275),"⌄",24,MUTED,False,"mm")
    packages=["7-Zip","Firefox","VLC media player","LibreOffice","HWiNFO","CrystalDiskInfo","PuTTY","TreeSize Free"]
    for i,n in enumerate(packages):
        x=100+(i%2)*420; y=340+(i//2)*58; rr(d,(x,y,x+390,y+44),7,WHITE,LINE); rr(d,(x+12,y+12,x+32,y+32),4,BLUE); text(d,(x+22,y+22),"✓",14,WHITE,True,"mm"); text(d,(x+45,y+22),n,17,INK,False,"lm")
    rr(d,(940,245,1175,305),9,BLUE); text(d,(1057,275),"8 logiciels choisis",17,WHITE,True,"mm")
    return im

def scene_simulation():
    im=Image.new("RGB",(W,H),(248,250,252)); d=ImageDraw.Draw(im); chrome(d)
    text(d,(100,158),"2. Vérifier en simulation",30,INK,True); text(d,(100,198),"Aucune installation n’est lancée tant que vous ne l’autorisez pas.",19,MUTED)
    rr(d,(100,250,570,314),10,(236,253,243),(171,239,198)); d.ellipse((126,273,144,291),fill=GREEN); text(d,(162,282),"MODE SIMULATION ACTIF",19,GREEN,True,"lm")
    rr(d,(610,250,1170,566),12,NAVY)
    logs=[("14:31:05","Vérification : 7-Zip"),("14:31:07","Vérification : Mozilla Firefox"),("14:31:09","Vérification : VLC media player"),("14:31:11","✓ Simulation : aucune installation exécutée")]
    for i,(t,m) in enumerate(logs): text(d,(638,292+i*59),f"[{t}]",16,(135,167,214),False); text(d,(748,292+i*59),m,16,WHITE,False)
    rr(d,(100,354,570,420),10,WHITE,LINE); text(d,(122,387),"☐ MODE EXÉCUTION",18,MUTED,True,"lm"); rr(d,(100,452,570,518),10,INK); text(d,(335,485),"PRÉPARER CE PC",18,WHITE,True,"mm")
    return im

def scene_execute():
    im=Image.new("RGB",(W,H),(248,250,252)); d=ImageDraw.Draw(im); chrome(d)
    text(d,(100,158),"3. Installer un logiciel",30,INK,True); text(d,(100,198),"Le mode réel demande une action volontaire.",19,MUTED)
    rr(d,(100,250,570,314),10,(255,244,229),(254,200,75)); rr(d,(126,272,146,292),4,BLUE); text(d,(136,282),"✓",14,WHITE,True,"mm"); text(d,(164,282),"MODE EXÉCUTION AUTORISÉ",19,INK,True,"lm")
    rr(d,(100,352,570,418),10,BLUE); text(d,(335,385),"PRÉPARER CE PC",18,WHITE,True,"mm")
    rr(d,(610,250,1170,530),12,NAVY); text(d,(640,286),"Installation : 7-Zip",20,WHITE,True); rr(d,(640,330,1140,350),10,(44,62,88)); rr(d,(640,330,1100,350),10,BLUE); text(d,(640,390),"Téléchargement terminé",17,(196,211,232)); text(d,(640,430),"Installation silencieuse…",17,(196,211,232)); text(d,(640,478),"✓ 7-Zip — Installation terminée",18,(110,231,183),True)
    return im

def scene_pro():
    im=Image.new("RGB",(W,H),(239,244,255)); d=ImageDraw.Draw(im); chrome(d,pro=True)
    text(d,(100,158),"4. Gérer les licences Pro",30,INK,True); text(d,(100,198),"Suivez les places disponibles sans exposer les clés dans les journaux.",19,MUTED)
    headers=[("Produit",110),("Total",565),("Utilisées",690),("Disponibles",845),("État",1010)]
    for h,x in headers: text(d,(x,270),h,16,MUTED,True)
    rows=[("CCleaner Professional","10","7","3","Stock faible",(255,250,235)),("Avast Business","20","12","8","Disponible",(236,253,243)),("Microsoft 365","5","5","0","Épuisé",(254,243,242))]
    for i,row in enumerate(rows):
        y=300+i*76; rr(d,(100,y,1170,y+60),8,row[5]); text(d,(115,y+30),row[0],17,INK,True,"lm");
        for val,x in zip(row[1:5],[588,735,890,1025]): text(d,(x,y+30),val,16,INK,False,"mm" if x<1000 else "lm")
    rr(d,(100,548,335,606),9,BLUE); text(d,(217,577),"ATTRIBUER UNE LICENCE",15,WHITE,True,"mm")
    return im

def scene_custom():
    im=Image.new("RGB",(W,H),(248,250,252)); d=ImageDraw.Draw(im); chrome(d,pro=True)
    text(d,(100,158),"5. Ajouter CCleaner avec WinGet",30,INK,True); text(d,(100,198),"Un utilisateur Pro peut compléter localement le catalogue.",19,MUTED)
    labels=[("Nom du logiciel","CCleaner"),("Identifiant WinGet","Piriform.CCleaner"),("Catégorie","Personnalisé"),("Détection","CCleaner64.exe")]
    for i,(lab,val) in enumerate(labels):
        y=252+i*74; text(d,(105,y),lab,15,MUTED,True); rr(d,(310,y-10,800,y+42),8,WHITE,LINE); text(d,(330,y+16),val,18,INK,False,"lm")
    rr(d,(850,252,1170,448),12,(239,244,255),(180,205,255)); text(d,(880,285),"IDENTIFIANT VÉRIFIÉ",15,BLUE,True); wrap(d,(880,325),"IPT Toolbox utilise l’identifiant exact et la source WinGet.",250,18,MUTED)
    rr(d,(850,480,1170,538),9,BLUE); text(d,(1010,509),"AJOUTER AU CATALOGUE",15,WHITE,True,"mm")
    return im

def scene_security():
    im,d=base_scene("SÉCURITÉ PAR DÉFAUT","Vous gardez le contrôle.","Simulation active au démarrage, clés Pro signées, coffre chiffré par Windows et aucune licence commerciale publiée sur GitHub.")
    items=[("Simulation","Aucune action réelle par défaut"),("Chiffrement","Coffre lié au compte Windows"),("Activation","Fausses clés Pro refusées"),("Transparence","Code source disponible sur GitHub")]
    for i,(a,b) in enumerate(items): x=680+(i%2)*250; y=120+(i//2)*220; rr(d,(x,y,x+220,y+180),18,WHITE,LINE); rr(d,(x+24,y+22,x+66,y+64),12,(239,244,255)); text(d,(x+45,y+43),"✓",20,BLUE,True,"mm"); text(d,(x+24,y+88),a,20,INK,True); wrap(d,(x+24,y+120),b,170,15,MUTED)
    return im

def scene_end():
    im=Image.new("RGB",(W,H),NAVY); d=ImageDraw.Draw(im)
    rr(d,(80,86,150,156),16,BLUE); text(d,(115,121),"IPT",20,WHITE,True,"mm")
    text(d,(80,220),"IPT Toolbox Sartori",52,WHITE,True); wrap(d,(80,292),"Préparez vos PC Windows et suivez vos licences professionnelles.",780,28,(194,210,232))
    rr(d,(80,430,520,500),12,BLUE); text(d,(300,465),"TÉLÉCHARGER GRATUITEMENT",18,WHITE,True,"mm")
    text(d,(80,548),"venioo20.github.io/IPT-Toolbox-Sartori",22,(182,201,230),True)
    text(d,(80,600),"IPT Toolbox Pro : 9,90 € / mois ou 79,90 € définitif",18,(135,167,214))
    return im

scenes=[scene_intro(),scene_profile(),scene_simulation(),scene_execute(),scene_pro(),scene_custom(),scene_security(),scene_end()]
durations=[12,15,15,16,16,18,14,14]
assert sum(durations)==TOTAL

ffmpeg=imageio_ffmpeg.get_ffmpeg_exe()
silent=OUT/"ipt-toolbox-demo-silent.mp4"
cmd=[ffmpeg,"-y","-f","rawvideo","-vcodec","rawvideo","-pix_fmt","rgb24","-s",f"{W}x{H}","-r",str(FPS),"-i","-","-an","-vcodec","libx264","-preset","medium","-crf","20","-pix_fmt","yuv420p",str(silent)]
p=subprocess.Popen(cmd,stdin=subprocess.PIPE)
for idx,(scene,duration) in enumerate(zip(scenes,durations)):
    frames=duration*FPS
    for n in range(frames):
        frame=scene
        if n>=frames-8 and idx+1<len(scenes):
            alpha=(n-(frames-8)+1)/9
            frame=Image.blend(scene,scenes[idx+1],alpha)
        p.stdin.write(frame.tobytes())
p.stdin.close()
if p.wait()!=0: raise SystemExit("Encodage vidéo échoué")
print(silent)
