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

//�ڽ��� ����
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

        //���⼭ ��Ģ�Է�

        if(move.success)
        {
            Destroy(board[move.removeY, move.removeX]);
            board[move.removeY, move.removeX] = null;
        }

        if (type == PiceType.KING)
        {
            return;
        }

        //���� �ݴ��� ������ ���� ŷ���� �ٲ��.
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

        //�ϳ��� ������ ������ ���� ��� false��ȯ
        if((x < 0 || x >= cols) ||
            (y < 0 || y >= rows))
        {
            return false;
        }

        return true;
    }

    //������ �� ��� �������� ���ϴ� �Լ�
    public Move[] GetMoves(ref PieceDraughts[,] board)
    {
        List<Move> moves = new List<Move>();

        //���� ���� �ƴѰ�찡 �������� ������
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

                    //�츮 ���ϰ�� �����ε���
                    if ((p != null) && p.color == color)
                    {
                        break;
                    }

                    Move m = new Move();
                    m.piece = this;

                    //���� ���� ���
                    if(p == null)
                    {
                        m.x = nextX;
                        m.y = nextY;
                    }
                    else
                    {
                        int hopX = nextX + mX;
                        int hopY = nextY + mY;

                        //��ܰų� �ٷ� �ڿ� ���� �ִ°�쵵 �����ε���
                        if (!IsMoveBounds(hopX, hopY, ref board) || (board[hopY, hopX] != null))
                            break;

                        //���� �̵����� ����
                        m.x = hopX;
                        m.y = hopY;

                        m.success = true;

                        //������ �� ��ġ ����
                        m.removeX = nextX;
                        m.removeY = nextY;

                        //����Ʈ�� �ൿ�߰�
                        GetMinMove(m, ref board);
                        moves.Add(m);
                        break;
                    }

                    //����Ʈ�� �ൿ�߰�
                    GetMinMove(m, ref board);
                    moves.Add(m);

                    //����������� ��ĭ �� ����.
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

        //�������� �����ʴ밢�� �� ���ʴ밣�� �ΰ��� �������� ������.
        foreach (int mX in moveX)
        {
            //�̵������� ��ġ�� ����ϱ� ���� ���ο� ���� ����
            int nextX = x + mX;
            int nextY = y + moveY;

            if(!IsMoveBounds(nextX, nextY, ref board))
            {
                //����� ��� �����ε���
                continue;
            }

            //������ ������ ���� Ȯ��
            PieceDraughts p = board[nextY, nextX];

            //�츮 ���ϰ�� �����ε���
            if(p != null && p.color == color)
            {
                continue;
            }

            Move m = new Move();
            m.piece = this;

            //�� ������ ���� ���� ���� ���
            if (p == null)
            {
                m.x = nextX;
                m.y = nextY;
            }
            //������� �ִ� ���
            else
            {
                int hopX = nextX + mX;
                int hopY = nextY + moveY;

                //��ܰų� �ٷ� �ڿ� ���� �ִ°�쵵 �����ε���
                if (!IsMoveBounds(hopX, hopY, ref board) || (board[hopY, hopX] != null))
                    continue;

                //���� �̵����� ����
                m.x = hopX;
                m.y = hopY;

                m.success = true;

                //������ �� ��ġ ����
                m.removeX = nextX;
                m.removeY = nextY;
            }

            GetMinMove(m, ref board);
            moves.Add(m);
        }

        return moves;
    }

    //���� ���� ����� ���� �Ͽ� �������� ������������ �˻�
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
            //���� �������� �������� �˻縦 �Ѵ�.
            int nextX = move.x + mX;
            int nextY = move.y + moveY;

            PieceDraughts p = null;

            //�ܰ� �˻縦 ����
            if (!IsMoveBounds(nextX, nextY, ref board))
            {
                continue;
            }

            //�ڽ��� �տ� ���� �˻�
            p = board[nextY, nextX];

            //���ų� ���� ���̸� ���� �ε����� �Ѿ 
            if (p == null || p.color == color)
            {
                continue;
            }

            //-----------------���� ���� ���渻�� ���------------------------------

            //������ ������������ ������� �Ǵ�
            if (!IsMoveBounds(move.x - mX, move.y - moveY, ref board))
            {
                continue;
            }

            //�� �ڿ� �ƹ��͵� ���ų� �� �ڽ��� ��� �����°� Ȯ��
            if ((board[move.y - moveY, move.x - mX] == null) || (board[move.y - moveY, move.x - mX] == this))
            {
                move.NextRemove = true;
            }

            //���������� �ڽ��� ��������� �������� �� ���⿡ ���� �ִ����� Ȯ��
            if (move.NextRemove)
            {
                if (IsMoveBounds(move.x - mX, move.y - moveY, ref board))
                {
                    p = board[move.y - moveY, move.x - mX];

                    //���� ���ų� ���� ���̸� Ż��
                    if (p == null || p.color == color)
                    {
                        continue;
                    }

                    //������ ŷ�� �ƴ϶�� Ż��
                    if (p.type != PiceType.KING)
                    {
                        continue;
                    }

                    //ŷ�� ��� �ڽ��� ���� �� �ִ����� �Ǵ�
                    if (board[nextY, nextX] == null)
                    {
                        //������ ��Ȳ
                        move.NextRemove = true;
                    }
                }
            }
        }
    }
}
