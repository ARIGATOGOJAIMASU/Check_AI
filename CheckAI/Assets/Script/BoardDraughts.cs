using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BoardDraughts : Board
{
    //Player의 상태
    public enum PlayerCoice { NONE, COICE }

    private int size = 8;
    private int numPieces = 12;

    //말로 쓰일 객체
    public GameObject[] prefab;

    //보드판
    protected PieceDraughts[,] board;

    //Player하는 객체들
    public PlayerCheck Player;
    public PlayerCheck AI;

    private List<Move> listMove = new List<Move>();

    //Ai 실행속도의 Delay를 주기위한 용도
    [SerializeField] float DelayTime;

    //디버깅을 위해서 이름을 지정할려고만듬
    private int WhiteCount;
    private int BlackCount;

    //player가 마우스로 클릭한 좌표를 받음
    private Vector2 playerCoice;

    private PlayerCoice playerstate = PlayerCoice.NONE;

    //사용해야하는 말을 캐싱
    private PieceDraughts curPiece;

    //Player의 가능한 움직임을 표시하기위한 객체
    public GameObject[] distractor;

    //연속해서 호출하는 것을 막기 위해 캐싱을 하여 사용여부를 체크중
    public Coroutine AiCoroutine;

    //상대방말을 잡았을시 활성화
    public bool Bonus;

    //현재 turn표시를 위한  UI_Text
    public GameObject PlayerTurn;
    public GameObject AITurn;

    //게임이 진행 할 수 있는 상황에는 항상 true;
    public bool IsGamePlaying;

    //게임이 끝날시 활성화하는 UI
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

        //하얀색 생성
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

        //검은색 생성
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

    //말들의 위치 지정
    private void PlacePieces(int x, int y, PiceColor color)
    {
        GameObject go = GameObject.Instantiate(prefab[(int)color]);

        go.transform.position = GetPosition(x, y);

        PieceDraughts p = go.GetComponent<PieceDraughts>();

        //piece초기화
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

    //가치가 제일 높은 움직임을 반환
    private Move Evaluate(Move[] moves)
    {
        List<Move> possibleMoves = new List<Move>();
        List<Move> listLastMoves = new List<Move>();
        Move GoodMove = null;
        bool success = false;

        foreach (Move mv in moves)
        {
            //적을 잡는 경우를 우선순위 최상위로 두고 저장.
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

        //다음움직임에 안 잡히는 선택지가 있는지 확인
        foreach (Move mv in possibleMoves)
        {
            if (!mv.NextRemove)
            {
                listLastMoves.Add(mv);
            }
        }

        //안잡히는 경우의 수가 없는 경우 전 리스트 Copy
        if (listLastMoves.Count == 0)
        {
            listLastMoves = possibleMoves;
        }

        int BicY = size;

        //제일 끝하고 가까운 말 찾기
        foreach (Move mv in listLastMoves)
        {
            if(BicY > mv.y && mv.piece.type ==PiceType.MAN)
            {
                GoodMove = mv;
            }
        }

        return GoodMove == null ? listLastMoves[Random.Range(0, listLastMoves.Count)] : GoodMove;
    }

    //이 말의 가능한 움직임들을 반환
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
                                //이동가능한 말을 고를때
                                if ((Piece.x == playerCoice.x) && (Piece.y == playerCoice.y))
                                {
                                    Move[] moves = Piece.GetMoves(ref board);

                                    //들어가기전 보기 reset
                                    ResetExample();

                                    //현재 선택된 말을 캐싱
                                    curPiece = Piece;

                                    if (moves.Length != 0)
                                    {
                                        //보기 시각화
                                        Example(moves);

                                        //선택을 할 수 있으므로 Coice단계로 넘어감
                                        playerstate = PlayerCoice.COICE;
                                    }

                                    break;
                                }
                            }
                        }
                        else if (playerstate == PlayerCoice.COICE)
                        {
                            //선택한 말의 이동가능한 움직임들을 반환
                            Move[] moves = curPiece.GetMoves(ref board);

                            //움직이기 전 보기 reset
                            ResetExample();

                            //이동 가능한 선택지인지 검사
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
                    //전 순서에서 상대말을 잡은 경우
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
            //재시작
            if(Input.GetKeyDown(KeyCode.Space))
            {
                SceneManager.LoadScene(0);
            }
        }
    }

    public void MoveingPiece(Move move, PlayerCheck checks)
    {
        //움직일 말
        PieceDraughts p = move.piece;

        //board의 정보 수정
        board[move.y, move.x] = p;
        board[p.y, p.x] = null;

        p.x = move.x;
        p.y = move.y;

        //이동
        p.transform.position = GetPosition(move.x, move.y);

        //말을 잡았는지 확인.
        if (move.success)
        {
            //해당 말을 삭제 그후 이동
            checks.MyPiece.Remove(board[move.removeY, move.removeX]);
            Destroy(board[move.removeY, move.removeX].gameObject);
            board[move.removeY, move.removeX] = null;

            //삭제 후 상대방 말이 남아 있는지를 Check 없으면 해당 플레이어가 승리
            if (PieceCheck())
            {
                GameOver(player);
            }

            Bonus = true;
            curPiece = p;

            bool NextChance = false;

            //다음 경우의 수를 탐색
            Move[] moves = curPiece.GetMoves(ref board);

            foreach (Move NextMove in moves)
            {
                if (NextMove.success)
                {
                    NextChance = true;
                    break;
                }
            }

            //다음에도 잡을 수 있는 경우
            if (NextChance)
            {
                if (player == 1)
                {
                    Example(moves);
                }
            }
            //잡지 못하는 경우 차례를 넘김
            else
            {
                TurnSwap();
            }
        }
        //상대방 말을 못 잡은 경우 순서를 바꿔준다.
        else
        {
            TurnSwap();
        }

        //끝에 도착한 경우 킹으로 변경
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

            //위치로 이동시킨다음 활성화
            distractor[i].transform.position = GetPosition(moves[i].x, moves[i].y);
            distractor[i].SetActive(true);
        }
    }

    public void ResetExample()
    {
        for (int i = 0; i < distractor.Length; ++i)
        {
            //위치로 이동시킨다음 활성화
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

        //승리 문구 띄우기
        WinText[win].SetActive(true);
    }

    IEnumerator AiTurn()
    {
        yield return new WaitForSeconds(DelayTime);

        Move LastMove;

        //모든 가능성이 있는 움직임을 받는다.
        listMove.AddRange(GetMoves(AI));

        //보너스인지 확인
        if (!Bonus)
        {
            //가능성중 제일 높은 가능성의 움직임을 반환
            LastMove = Evaluate(listMove.ToArray());
        }
        else
        {
            //보너스인 경우 보너스를 얻은 말만 움직이게 설정
            LastMove = Evaluate(curPiece.GetMoves(ref board));
            
            if(!LastMove.success)
            {
                LastMove = null;
            }
        }

        if (LastMove != null)
        {
            //움직임을 실행
            MoveingPiece(LastMove, Player);
        }
        else
        {
            if (Bonus)
            {
                //시행할 움직임이 없는 경우 차례를 넘김
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
