using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BoardDraughts : Board
{
    //Player�� ����
    public enum PlayerCoice { NONE, COICE }

    private int size = 8;
    private int numPieces = 12;

    //���� ���� ��ü
    public GameObject[] prefab;

    //������
    protected PieceDraughts[,] board;

    //Player�ϴ� ��ü��
    public PlayerCheck Player;
    public PlayerCheck AI;

    private List<Move> listMove = new List<Move>();

    //Ai ����ӵ��� Delay�� �ֱ����� �뵵
    [SerializeField] float DelayTime;

    //������� ���ؼ� �̸��� �����ҷ�����
    private int WhiteCount;
    private int BlackCount;

    //player�� ���콺�� Ŭ���� ��ǥ�� ����
    private Vector2 playerCoice;

    private PlayerCoice playerstate = PlayerCoice.NONE;

    //����ؾ��ϴ� ���� ĳ��
    private PieceDraughts curPiece;

    //Player�� ������ �������� ǥ���ϱ����� ��ü
    public GameObject[] distractor;

    //�����ؼ� ȣ���ϴ� ���� ���� ���� ĳ���� �Ͽ� ��뿩�θ� üũ��
    public Coroutine AiCoroutine;

    //���渻�� ������� Ȱ��ȭ
    public bool Bonus;

    //���� turnǥ�ø� ����  UI_Text
    public GameObject PlayerTurn;
    public GameObject AITurn;

    //������ ���� �� �� �ִ� ��Ȳ���� �׻� true;
    public bool IsGamePlaying;

    //������ ������ Ȱ��ȭ�ϴ� UI
    public GameObject WinBoard;
    public GameObject[] WinText;
    private void Awake()
    {
        board = new PieceDraughts[size, size];
    }

    private void Start()
    {
        for (int l = 0; l < 2; ++l)
        {
            PieceDraughts pd = prefab[l].GetComponent<PieceDraughts>();

            if (pd == null)
            {
                Debug.LogError("no PieceDraugh componenet derected");
                return;
            }
        }

        int i;
        int j;

        //�Ͼ�� ����
        int piecesLeft = numPieces;

        for(i = 0; i < size; ++i)
        {
            if (piecesLeft == 0)
                break;

            int init = 0;

            if ((i % 2) != 0)
                init = 1;

            for(j = init; j < size; j+=2)
            {
                if(piecesLeft == 0)
                {
                    break;
                }

                PlacePieces(j, i, PiceColor.WHITE);
                piecesLeft--;
            }
        }

        //������ ����
        piecesLeft = numPieces;

        for (i = size - 1; i >= 0; --i)
        {
            if (piecesLeft == 0)
                break;

            int init = 0;

            if ((i % 2) != 0)
                init = 1;

            for (j = init; j < size; j += 2)
            {
                if (piecesLeft == 0)
                {
                    break;
                }

                PlacePieces(j, i, PiceColor.BLACK);
                piecesLeft--;
            }
        }
    }

    //������ ��ġ ����
    private void PlacePieces(int x, int y, PiceColor color)
    {
        GameObject go = GameObject.Instantiate(prefab[(int)color]);

        go.transform.position = GetPosition(x, y);

        PieceDraughts p = go.GetComponent<PieceDraughts>();

        //piece�ʱ�ȭ
        p.SetUp(x, y, color);
        board[y, x] = p;

        if(color == PiceColor.WHITE)
        {
            ++WhiteCount;
            p.name = "white" + WhiteCount.ToString();
            Player.MyPiece.Add(p);
        }
        else
        {
            ++BlackCount;
            p.name = "Blak" + BlackCount.ToString();
            AI.MyPiece.Add(p);
        }
    }

    //��ġ�� ���� ���� �������� ��ȯ
    private Move Evaluate(Move[] moves)
    {
        List<Move> possibleMoves = new List<Move>();
        List<Move> listLastMoves = new List<Move>();
        Move GoodMove = null;
        bool success = false;

        foreach (Move mv in moves)
        {
            //���� ��� ��츦 �켱���� �ֻ����� �ΰ� ����.
            if (mv != null)
            {
                if (mv.success)
                {
                    if (!success)
                    {
                        success = true;
                        possibleMoves.Clear();
                    }

                    possibleMoves.Add(mv);
                }
                else if (!success)
                {
                    possibleMoves.Add(mv);
                }
            }
        }

        //���������ӿ� �� ������ �������� �ִ��� Ȯ��
        foreach (Move mv in possibleMoves)
        {
            if (!mv.NextRemove)
            {
                listLastMoves.Add(mv);
            }
        }

        //�������� ����� ���� ���� ��� �� ����Ʈ Copy
        if (listLastMoves.Count == 0)
        {
            listLastMoves = possibleMoves;
        }

        int BicY = size;

        //���� ���ϰ� ����� �� ã��
        foreach (Move mv in listLastMoves)
        {
            if(BicY > mv.y && mv.piece.type ==PiceType.MAN)
            {
                GoodMove = mv;
            }
        }

        return GoodMove == null ? listLastMoves[Random.Range(0, listLastMoves.Count)] : GoodMove;
    }

    //�� ���� ������ �����ӵ��� ��ȯ
    public override Move[] GetMoves(PlayerCheck playerCheck)
    {
        List<Move> moves = new List<Move>();

        for (int i = 0; i < playerCheck.MyPiece.Count; ++i)
        {
            moves.AddRange((playerCheck.MyPiece[i].GetMoves(ref board)));
        }

        return moves.ToArray();
    }


    private void Update()
    {
        if (IsGamePlaying)
        {
            if (player == 1)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Ray MousRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                    playerCoice = new Vector2((int)(MousRay.origin.x * 0.1f), (int)(MousRay.origin.z * 0.1f));

                    if (!Bonus)
                    {
                        if (playerstate == PlayerCoice.NONE)
                        {
                            foreach (PieceDraughts Piece in Player.MyPiece)
                            {
                                //�̵������� ���� ����
                                if ((Piece.x == playerCoice.x) && (Piece.y == playerCoice.y))
                                {
                                    Move[] moves = Piece.GetMoves(ref board);

                                    //������ ���� reset
                                    ResetExample();

                                    //���� ���õ� ���� ĳ��
                                    curPiece = Piece;

                                    if (moves.Length != 0)
                                    {
                                        //���� �ð�ȭ
                                        Example(moves);

                                        //������ �� �� �����Ƿ� Coice�ܰ�� �Ѿ
                                        playerstate = PlayerCoice.COICE;
                                    }

                                    break;
                                }
                            }
                        }
                        else if (playerstate == PlayerCoice.COICE)
                        {
                            //������ ���� �̵������� �����ӵ��� ��ȯ
                            Move[] moves = curPiece.GetMoves(ref board);

                            //�����̱� �� ���� reset
                            ResetExample();

                            //�̵� ������ ���������� �˻�
                            foreach (Move move in moves)
                            {
                                if ((move.x == playerCoice.x) && (move.y == playerCoice.y))
                                {
                                    MoveingPiece(move, AI);
                                    break;
                                }
                            }

                            playerstate = PlayerCoice.NONE;
                        }
                    }
                    //�� �������� ��븻�� ���� ���
                    else
                    {
                        Move[] moves = curPiece.GetMoves(ref board);

                        foreach (Move move in moves)
                        {
                            if ((move.x == playerCoice.x) && (move.y == playerCoice.y))
                            {
                                if (move.success)
                                {
                                    MoveingPiece(move, AI);
                                    break;
                                }
                            }
                        }

                        ResetExample();
                    }
                }
            }
            //Ai Turn
            else
            {
                if (AiCoroutine == null)
                {
                    AiCoroutine = StartCoroutine(AiTurn());
                }
            }
        }
        else
        {
            //�����
            if(Input.GetKeyDown(KeyCode.Space))
            {
                SceneManager.LoadScene(0);
            }
        }
    }

    public void MoveingPiece(Move move, PlayerCheck checks)
    {
        //������ ��
        PieceDraughts p = move.piece;

        //board�� ���� ����
        board[move.y, move.x] = p;
        board[p.y, p.x] = null;

        p.x = move.x;
        p.y = move.y;

        //�̵�
        p.transform.position = GetPosition(move.x, move.y);

        //���� ��Ҵ��� Ȯ��.
        if (move.success)
        {
            //�ش� ���� ���� ���� �̵�
            checks.MyPiece.Remove(board[move.removeY, move.removeX]);
            Destroy(board[move.removeY, move.removeX].gameObject);
            board[move.removeY, move.removeX] = null;

            //���� �� ���� ���� ���� �ִ����� Check ������ �ش� �÷��̾ �¸�
            if (PieceCheck())
            {
                GameOver(player);
            }

            Bonus = true;
            curPiece = p;

            bool NextChance = false;

            //���� ����� ���� Ž��
            Move[] moves = curPiece.GetMoves(ref board);

            foreach (Move NextMove in moves)
            {
                if (NextMove.success)
                {
                    NextChance = true;
                    break;
                }
            }

            //�������� ���� �� �ִ� ���
            if (NextChance)
            {
                if (player == 1)
                {
                    Example(moves);
                }
            }
            //���� ���ϴ� ��� ���ʸ� �ѱ�
            else
            {
                TurnSwap();
            }
        }
        //���� ���� �� ���� ��� ������ �ٲ��ش�.
        else
        {
            TurnSwap();
        }

        //���� ������ ��� ŷ���� ����
        if (p.type != PiceType.KING)
        {
            if (((p.color == PiceColor.WHITE) && (p.y == 7)) ||
                 ((p.color == PiceColor.BLACK) && (p.y == 0)))
            {
                p.type = PiceType.KING;
                p.CrawonSprite.SetActive(true);

                if((player == 1) && (p.color ==  PiceColor.WHITE))
                {
                    TurnSwap();
                }
            }
        }

        listMove.Clear();
    }

    public Vector3 GetPosition(int x, int y)
    {
        return new Vector3((5 + x * 10), 0, (5 + y * 10));
    }

    public void Example(Move[] moves)
    {
        ResetExample();

        for (int i = 0; i < moves.Length; ++i)
        {
            if (Bonus)
            {
                if(!moves[i].success)
                {
                    continue;
                }
            }

            //��ġ�� �̵���Ų���� Ȱ��ȭ
            distractor[i].transform.position = GetPosition(moves[i].x, moves[i].y);
            distractor[i].SetActive(true);
        }
    }

    public void ResetExample()
    {
        for (int i = 0; i < distractor.Length; ++i)
        {
            //��ġ�� �̵���Ų���� Ȱ��ȭ
            distractor[i].SetActive(false);
        }
    }

    public void TurnSwap()
    {
        if (player == 0)
        {
            player = 1;

            PlayerTurn.SetActive(true);
            AITurn.SetActive(false);
        }
        else
        {
            ResetExample();

            player = 0;

            PlayerTurn.SetActive(false);
            AITurn.SetActive(true);
        }

        if(Bonus)
        {
            Bonus = false;
        }
    }

    public bool PieceCheck()
    {
        if((player == 1) && (AI.MyPiece.Count == 0))
        {
            return true;
        }
        else if((player == 0) && (Player.MyPiece.Count == 0))
        {
            return true;
        }

        return false;
    }

    public void GameOver(int win)
    {
        IsGamePlaying = false;
        WinBoard.SetActive(true);

        //�¸� ���� ����
        WinText[win].SetActive(true);
    }

    IEnumerator AiTurn()
    {
        yield return new WaitForSeconds(DelayTime);

        Move LastMove;

        //��� ���ɼ��� �ִ� �������� �޴´�.
        listMove.AddRange(GetMoves(AI));

        //���ʽ����� Ȯ��
        if (!Bonus)
        {
            //���ɼ��� ���� ���� ���ɼ��� �������� ��ȯ
            LastMove = Evaluate(listMove.ToArray());
        }
        else
        {
            //���ʽ��� ��� ���ʽ��� ���� ���� �����̰� ����
            LastMove = Evaluate(curPiece.GetMoves(ref board));
            
            if(!LastMove.success)
            {
                LastMove = null;
            }
        }

        if (LastMove != null)
        {
            //�������� ����
            MoveingPiece(LastMove, Player);
        }
        else
        {
            if (Bonus)
            {
                //������ �������� ���� ��� ���ʸ� �ѱ�
                Bonus = false;
                player = 1;
            }
            else
            {
                GameOver(1);
            }
        }

        AiCoroutine = null;
    }
}
