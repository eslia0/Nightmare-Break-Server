using System;
using System.Collections;

	public class EmptyClass
	{
		int[] intArr1 = new int[5];
		int[] intArr2 = new int[5];

		public EmptyClass ()
		{
			intArr1 [0] = 1;
			intArr1 [1] = 2;
			intArr1 [2] = 3;
			intArr1 [3] = 4;
			intArr1 [4] = 5;
			intArr2 [0] = 6;
			intArr2 [1] = 7;
			intArr2 [2] = 8;
			intArr2 [3] = 9;
			intArr2 [4] = 0;			
		}

		public void PrintAllArray(){
			for(int i = 0; i< intArr1.Length; i++){
				Console.WriteLine (intArr1[i]);
			}
			for(int i = 0; i< intArr2.Length; i++){
				Console.WriteLine (intArr2[i]);
			}
		}

		public static void Main(string[] args){
			EmptyClass emptyClass = new EmptyClass ();
			emptyClass.PrintAllArray ();
		}
	}
