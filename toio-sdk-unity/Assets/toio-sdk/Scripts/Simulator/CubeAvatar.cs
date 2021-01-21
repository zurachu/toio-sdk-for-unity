using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace toio.Simulator
{
    public class CubeAvatar : MonoBehaviour
    {
        // ======= Settings ========
        public enum Role
        {
            DIE,
            SIM,
            REAL
        }
        [SerializeField]
        public Role roleNonEditor = Role.DIE;
        public Role roleEditor = Role.SIM;
        public Role role { get
        {
            #if UNITY_EDITOR
            return roleEditor;
            #else
            return roleNonEditor;
            #endif
        } }

        private bool isEditor = false;

        // ======= Objects ========
        private Rigidbody rb;
        private AudioSource audioSource;
        private GameObject cubeModel;
        private GameObject LED;
        private BoxCollider col;


        // ====== REAL 用 ======
        private static List<CubeAvatar> realUnusedAvatars = new List<CubeAvatar>();
        private static List<CubeAvatar> realUsedAvatars = new List<CubeAvatar>();
        private static Dictionary<CubeAvatar, Cube> dictAvatarCube = new Dictionary<CubeAvatar, Cube>();
        private static Dictionary<Cube, CubeAvatar> dictCubeAvatar = new Dictionary<Cube, CubeAvatar>();
        public Mat mat;
        public string matchRealID = "";


        void Start()
        {
            this.rb = GetComponent<Rigidbody>();
            this.rb.maxAngularVelocity = 21f;
            this.audioSource = GetComponent<AudioSource>();
            this.LED = transform.Find("LED").gameObject;
            this.LED.GetComponent<Renderer>().material.color = Color.black;
            this.cubeModel = transform.Find("cube_model").gameObject;
            this.col = GetComponent<BoxCollider>();

            this.InitPresetSounds();

            #if UNITY_EDITOR
            this.isEditor = true;
            #endif

            if (role == Role.DIE) this.gameObject.SetActive(false);
            if (role == Role.REAL)
            {
                this.GetComponent<CubeSimulator>().enabled = false;

                realUnusedAvatars.Add(this);
                this.gameObject.SetActive(false);
            }
        }

        void FixedUpdate()
        {
            if (role == Role.REAL)
            {
                UpdateWithReal();
            }
        }

        // ====== REAL 用関数 ======


        internal void UpdateWithReal()
        {
            // TODO
        }


        // ---- Transform ----
        private Func<GameObject, Vector3Int, (Vector3, Vector3)> _coordTransromFunc = DefaultCoordTransformFunc;
        public void SetCoordTransfomFunc(Func<GameObject, Vector3Int, (Vector3, Vector3)> func)
        {
            this._coordTransromFunc = func;
        }
        private static (Vector3, Vector3) DefaultCoordTransformFunc(GameObject obj, Vector3Int coord_deg)
        {
            var loc = Vector3.zero;
            var deg = Vector3.zero;

            var mat = obj.GetComponent<Mat>();
            loc = Mat.MatCoord2UnityCoord(coord_deg.x, coord_deg.y, mat);
            deg = new Vector3(0, Mat.MatDeg2UnityDeg(coord_deg.z, mat), 0);
            return (loc, deg);
        }

        // ---- Real Callbacks ----
        private void OnUpdateID(Cube cube)
        {
            Vector3 locU, degU;
            (locU, degU) = _coordTransromFunc(this.mat.gameObject, new Vector3Int(cube.x, cube.y, cube.angle));
            this.transform.position = locU;
            this.transform.eulerAngles = degU;
        }


        // ---- Pairing ----
        public void InitWithRealCube(Cube cube)
        {
            this.gameObject.SetActive(true);
            realUnusedAvatars.Remove(this);
            realUsedAvatars.Add(this);
            dictAvatarCube.Add(this, cube);
            dictCubeAvatar.Add(cube, this);

            // TODO
            string id = this.GetInstanceID().ToString();
            // cube.collisionCallback.AddListener(id, OnCollision);
            cube.idCallback.AddListener(id, OnUpdateID);
            // cube.standardIdCallback.AddListener(id, OnUpdateStandardId);
        }
        public void UninitWithRealCube(Cube cube)
        {
            this.gameObject.SetActive(false);
            realUnusedAvatars.Add(this);
            realUsedAvatars.Remove(this);
            dictAvatarCube.Remove(this);
            dictCubeAvatar.Remove(cube);

            // TODO
            string id = this.GetInstanceID().ToString();
            cube.idCallback.RemoveListener(id);
        }

        public static void PairCubes(List<Cube> cubes, bool depairNotListed=false)
        {
            foreach (var cube in cubes)
            {
                PairCube(cube);
            }
        }
        public static void PairCube(Cube cube)
        {
            CubeAvatar toPair = null;
            if (!dictCubeAvatar.ContainsKey(cube) && realUnusedAvatars.Count>0)
            {
                // Search for ID-Specified avatar
                foreach (var avatar in realUnusedAvatars)
                {
                    if (avatar.matchRealID == cube.id)
                    {
                        toPair = avatar; break;
                    }
                }

                // Search for a Not ID-Specified avatar
                if (toPair==null)
                foreach (var avatar in realUnusedAvatars)
                {
                    if (avatar.matchRealID == "")
                    {
                        toPair = avatar; break;
                    }
                }

                // Found
                if (toPair!=null)
                {
                    toPair.InitWithRealCube(cube);
                }
            }
        }
        public static void UnpairCube(Cube cube)
        {
            CubeAvatar toUnpair=null;
            if (dictCubeAvatar.ContainsKey(cube))
            {
                toUnpair = dictCubeAvatar[cube];
            }

            if (toUnpair!=null)
            {
                toUnpair.UninitWithRealCube(cube);
            }
        }



        // ====== SIM 用関数 ======

        // 速度変化によって力を与え、位置と角度を更新
        internal void SetSpeed(float speedL, float speedR)
        {
            this.rb.angularVelocity = transform.up * (float)((speedL - speedR) / CubeSimulator.TireWidthM);
            var vel = transform.forward * (speedL + speedR) / 2;
            var dv = vel - this.rb.velocity;
            this.rb.AddForce(dv, ForceMode.VelocityChange);
        }
        internal void SetLight(int r, int g, int b){
            r = Mathf.Clamp(r, 0, 255);
            g = Mathf.Clamp(g, 0, 255);
            b = Mathf.Clamp(b, 0, 255);
            LED.GetComponent<Renderer>().material.color = new Color(r/255f, g/255f, b/255f);
        }

        internal void StopLight(){
            LED.GetComponent<Renderer>().material.color = Color.black;
        }

        private int playingSoundId = -1;
        internal void PlaySound(int soundId, int volume){
            if (soundId >= 128) { StopSound(); playingSoundId = -1; return; }
            if (soundId != playingSoundId)
            {
                playingSoundId = soundId;
                int octave = (int)(soundId/12);
                int idx = (int)(soundId%12);
                var aCubeOnSlot = Resources.Load("Octave/" + (octave*12+9)) as AudioClip;
                audioSource.pitch = (float)Math.Pow(2, ((float)idx-9)/12);
                audioSource.clip = aCubeOnSlot;
            }
            audioSource.volume = (float)volume/256;
            if (!audioSource.isPlaying)
                audioSource.Play();
        }
        internal void StopSound(){
            audioSource.clip = null;
            audioSource.Stop();
        }

        // Sound Preset を設定
        internal void InitPresetSounds(){
            Cube.SoundOperation[] sounds = new Cube.SoundOperation[3];
            sounds[0] = new Cube.SoundOperation(200, 255, 48);
            sounds[1] = new Cube.SoundOperation(200, 255, 50);
            sounds[2] = new Cube.SoundOperation(200, 255, 52);
            // impl.presetSounds.Add(sounds);   // TODO
        }

        internal void SetPressed(bool pressed)
        {
            this.cubeModel.transform.localEulerAngles
                    = pressed? new Vector3(-93,0,0) : new Vector3(-90,0,0);
        }
    }

}
