using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PiceColor
{
    WHITE,
    BLACK
};

public enum PiceType
{
    MAN,
    KING
};

//자신의 병사
public class PieceDraughts : MonoBehaviour
{
    public int x;
    public int y;
    public PiceColor color;
    public PiceType type;
    public GameObject CrawonSprite;

    public void SetUp(int  x, int y, PiceColor color, PiceType type = PiceType.MAN)
    {
        this.x = x;
        this.y = y;
        this.color = color;
        this.type = type;
    }

    public void Move(Move move, ref PieceDraughts [,] board)
    {
        board[move.y, move.x] = this;
        board[y, x] = null;

        x = move.x;
        y = move.y;

        //여기서 규칙입력

        if(move.success)
        {
            Destroy(board[move.removeY, move.removeX]);
            board[move.removeY, move.removeX] = null;
        }

        if (type == PiceType.KING)
        {
            return;
        }

        //말이 반대편 끝까지 가면 킹으로 바뀐다.
        int rows = board.GetLength(0);

        if ((color == PiceColor.WHITE) && (y == rows))
            type = PiceType.KING;

        if((color == PiceColor.BLACK) && (y == 0))
            type = PiceType.KING;
    }

    private bool IsMoveBounds(int x, int y, ref PieceDraughts[,] board)
    {
        int rows = board.GetLength(0);
        int cols = board.GetLength(1);

        //하나라도 범위값 밖으로 나갈 경우 false반환
        if((x < 0 || x >= cols) ||
            (y < 0 || y >= rows))
        {
            return false;
        }

        return true;
    }

    //가능한 한 모든 움직임을 구하는 함수
    public Move[] GetMoves(ref PieceDraughts[,] board)
    {
        List<Move> moves = new List<Move>();

        //왕인 경우와 아닌경우가 선택지가 나눠짐
        if (type == PiceType.KING)
        {
            moves = GetMoveKing(ref board);
        }
        else
            moves = GetMoveMan(ref board);

        return moves.ToArray();
    }

    private List<Move> GetMoveKing(ref PieceDraughts[,] board)
    {
        List<Move> moves = new List<Move>(2);

        int[] moveX = new int[] { -1, 1 };
        int[] moveY = new int[] { -1, 1 };

        foreach(int mY in moveY)
        {
            foreach (int mX in moveX)
            {
                int nextX = x + mX;
                int nextY = y + mY;

                while(IsMoveBounds(nextX, nextY, ref board))
                {
                    PieceDraughts p = board[nextY, nextX];

                    //우리 팀일경우 다음인덱스
                    if ((p != null) && p.color == color)
                    {
                        break;
                    }

                    Move m = new Move();
                    m.piece = this;

                    //말이 없는 경우
                    if(p == null)
                    {
                        m.x = nextX;
                        m.y = nextY;
                    }
                    else
                    {
                        int hopX = nextX + mX;
                        int hopY = nextY + mY;

                        //장외거나 바로 뒤에 말이 있는경우도 다음인덱스
                        if (!IsMoveBounds(hopX, hopY, ref board) || (board[hopY, hopX] != null))
                            break;

                        //다음 이동방향 저장
                        m.x = hopX;
                        m.y = hopY;

                        m.success = true;

                        //삭제할 말 위치 저장
                        m.removeX = nextX;
                        m.removeY = nextY;

                        //리스트에 행동추가
                        GetMinMove(m, ref board);
                        moves.Add(m);
                        break;
                    }

                    //리스트에 행동추가
                    GetMinMove(m, ref board);
                    moves.Add(m);

                    //진행방향으로 한칸 더 간다.
                    nextX += mX;
                    nextY += mY;

                }
            }
        }

        return moves;
    }

    private List<Move> GetMoveMan(ref PieceDraughts[,] board)
    {
        List<Move> moves = new List<Move>();

        int[] moveX = new int[] { -1, 1 };
        int moveY = 1;

        if(color == PiceColor.BLACK)
        {
            moveY = -1;
        }

        //선택지는 오른쪽대각선 및 왼쪽대간선 두개의 선택지로 나뉜다.
        foreach (int mX in moveX)
        {
            //이동가능한 위치를 계산하기 위한 새로운 변수 선언
            int nextX = x + mX;
            int nextY = y + moveY;

            if(!IsMoveBounds(nextX, nextY, ref board))
            {
                //벗어나는 경우 다음인덱스
                continue;
            }

            //갈려는 방향의 말을 확인
            PieceDraughts p = board[nextY, nextX];

            //우리 팀일경우 다음인덱스
            if(p != null && p.color == color)
            {
                continue;
            }

            Move m = new Move();
            m.piece = this;

            //갈 방향의 돌이 말이 없는 경우
            if (p == null)
            {
                m.x = nextX;
                m.y = nextY;
            }
            //상대팀이 있는 경우
            else
            {
                int hopX = nextX + mX;
                int hopY = nextY + moveY;

                //장외거나 바로 뒤에 말이 있는경우도 다음인덱스
                if (!IsMoveBounds(hopX, hopY, ref board) || (board[hopY, hopX] != null))
                    continue;

                //다음 이동방향 저장
                m.x = hopX;
                m.y = hopY;

                m.success = true;

                //삭제할 말 위치 저장
                m.removeX = nextX;
                m.removeY = nextY;
            }

            GetMinMove(m, ref board);
            moves.Add(m);
        }

        return moves;
    }

    //상대방 말을 잡고나서 다음 턴에 잡히는지 안잡히는지를 검사
    public void GetMinMove(Move move, ref PieceDraughts[,] board)
    {
        int[] moveX = new int[] { -1, 1 };
        int moveY = 1;

        if (color == PiceColor.BLACK)
        {
            moveY = -1;
        }      

        foreach(int mX in moveX)
        {
            //다음 움직임을 기준으로 검사를 한다.
            int nextX = move.x + mX;
            int nextY = move.y + moveY;

            PieceDraughts p = null;

            //외각 검사를 시작
            if (!IsMoveBounds(nextX, nextY, ref board))
            {
                continue;
            }

            //자신의 앞에 말을 검사
            p = board[nextY, nextX];

            //없거나 같은 편이면 다음 인덱스로 넘어감 
            if (p == null || p.color == color)
            {
                continue;
            }

            //-----------------앞의 말이 상대방말인 경우------------------------------

            //상대방의 다음움직임이 장외인지 판단
            if (!IsMoveBounds(move.x - mX, move.y - moveY, ref board))
            {
                continue;
            }

            //내 뒤에 아무것도 없거나 내 자신인 경우 잡히는게 확정
            if ((board[move.y - moveY, move.x - mX] == null) || (board[move.y - moveY, move.x - mX] == this))
            {
                move.NextRemove = true;
            }

            //최종적으로 자신의 진행방향을 기준으로 뒷 방향에 왕이 있는지를 확인
            if (move.NextRemove)
            {
                if (IsMoveBounds(move.x - mX, move.y - moveY, ref board))
                {
                    p = board[move.y - moveY, move.x - mX];

                    //말이 없거나 같은 팀이면 탈출
                    if (p == null || p.color == color)
                    {
                        continue;
                    }

                    //상대방이 킹이 아니라면 탈출
                    if (p.type != PiceType.KING)
                    {
                        continue;
                    }

                    //킹일 경우 자신을 잡을 수 있는지를 판단
                    if (board[nextY, nextX] == null)
                    {
                        //잡히는 상황
                        move.NextRemove = true;
                    }
                }
            }
        }
    }
}
