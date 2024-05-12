using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private GameObject m_CurrentHero;
    private Vector2 m_CurrentPosition;
    private Vector2 m_NextPosition;
    private Vector2 m_facingDirection = Vector2.zero;

    private PlayerControl m_Controller; 
    private Vector2 m_MoveInput;

    public bool IsAction = false;
    private bool m_IsMoving = false;
    private bool m_IsRotating = false;

    private void Awake()
    {
        m_Controller = new PlayerControl();
    }

    private void Start()
    {
        m_CurrentPosition = Vector2.zero;
        m_NextPosition = Vector2.zero;
    }

    void Update()
    {
        ReadInput();

        if (!m_IsMoving && !m_IsRotating)
        {
            MovePlayer();

            if (Input.GetKeyDown(KeyCode.Q) || m_Controller.Gamepad.RotateLeft.triggered)
            {
                m_IsRotating = true;
                GameManager.Instance.RotateCharacters(true, () => m_IsRotating = false);
            }
            else if (Input.GetKeyDown(KeyCode.E) || m_Controller.Gamepad.RotateRight.triggered)
            {
                m_IsRotating = true;
                GameManager.Instance.RotateCharacters(false, () => m_IsRotating = false);
            }
        }

        if (m_IsMoving && !m_IsRotating)
        {
            m_CurrentHero.transform.position = Vector3.MoveTowards(m_CurrentHero.transform.position, m_NextPosition, Time.deltaTime);
            if (m_CurrentHero.transform.position == (Vector3)m_NextPosition)
            {
                m_CurrentPosition = m_NextPosition;
                GameManager.Instance.OnMove();
                m_IsMoving = false;
            }
        }
    }

    private void ReadInput()
    {
        m_MoveInput = Vector2.zero;

        m_MoveInput += new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        m_MoveInput += m_Controller.Gamepad.Move.ReadValue<Vector2>();
    }

    public void SetCurrentHero(GameObject hero, Vector2 facingDirection)
    {
        m_CurrentHero = hero;
        m_CurrentPosition = hero.transform.position;
        m_NextPosition = Vector2Int.zero;
        m_facingDirection = facingDirection;
    }

    private void MovePlayer()
    {
        Vector2 move = m_MoveInput.normalized;
        Vector2Int direction = Vector2Int.zero;

        if (move.x > 0)
        {
            direction = Vector2Int.right;
        }
        else if (move.x < 0)
        {
            direction = Vector2Int.left;
        }
        else if (move.y > 0)
        {
            direction = Vector2Int.up;
        }
        else if (move.y < 0)
        {
            direction = Vector2Int.down;
        }

        if (direction != Vector2Int.zero &&
            m_CurrentPosition.x + direction.x >= -7 && m_CurrentPosition.x + direction.x <= 8 &&
            m_CurrentPosition.y + direction.y >= -7 && m_CurrentPosition.y + direction.y <= 8 &&
            direction != -m_facingDirection)
        {
            m_NextPosition = m_CurrentPosition + direction;
            m_facingDirection = direction;
            m_IsMoving = true;
            GameManager.Instance.MovePlayer(m_CurrentPosition);
        }
    }
}
