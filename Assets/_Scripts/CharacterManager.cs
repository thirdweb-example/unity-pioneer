using System.Collections;
using Thirdweb;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Threading.Tasks;
using Thirdweb.Unity;

[RequireComponent(typeof(CharacterController))]
// Basic Character Movement
public class CharacterManager : MonoBehaviour
{
    [SerializeField]
    private float _speed = 1f;

    [SerializeField]
    private Animator _animator;

    [SerializeField]
    private AudioSource _footsteps;

    private CharacterController _characterController;
    private Vector3 _direction;
    private float _angle;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        Move();
        Animate();
        Footsteps();
    }

    private void Move()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        _direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (_direction.magnitude >= 0.01f)
        {
            float targetAngle = Mathf.Atan2(_direction.x, _direction.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(
                transform.eulerAngles.y,
                targetAngle,
                ref _angle,
                0.1f
            );

            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            _characterController.Move(_speed * Time.deltaTime * moveDirection);
        }
    }

    private void Animate()
    {
        _animator.SetBool("IsWalking", _direction.magnitude >= 0.01f);
    }

    private void Footsteps()
    {
        if (_direction.magnitude >= 0.01f && !_footsteps.isPlaying)
        {
            _footsteps.Play();
        }
        else if (_direction.magnitude < 0.01f && _footsteps.isPlaying)
        {
            _footsteps.Stop();
        }
    }

    private bool _triggered;

    private void OnTriggerEnter(Collider other)
    {
        if (!_triggered && other.CompareTag("Trigger_0"))
        {
            _triggered = true;
            ClaimAndLoad();
        }
    }

    private async void ClaimAndLoad()
    {
        LightenVoid();
        var contract = await ThirdwebManager.Instance.GetContract(
            BlockchainManager.TokenContract,
            BlockchainManager.ChainId
        );
        Debug.Log("Claiming");
        var smartWallet = BlockchainManager.SmartWallet;
        var externalWalletAddress = await (await smartWallet.GetPersonalWallet()).GetAddress();
        var claimTokensRes = await contract.DropERC20_Claim(
            smartWallet,
            externalWalletAddress,
            "100"
        );
        Debug.Log(claimTokensRes);
        StopAllCoroutines();
        SceneManager.LoadScene("02_Scene_Inventory");
    }

    private void LightenVoid()
    {
        StartCoroutine(Lighten());
    }

    private IEnumerator Lighten()
    {
        var light = GameObject.Find("Directional Light").GetComponent<Light>();
        while (light.intensity < 25f)
        {
            light.intensity += Time.deltaTime * 10f;
            yield return null;
        }
        while (light.intensity < 200f)
        {
            light.intensity += Time.deltaTime * 40f;
            yield return null;
        }
        Debug.Log("Lighten Done");
    }
}
