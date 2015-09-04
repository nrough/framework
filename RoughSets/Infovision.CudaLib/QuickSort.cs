using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cudafy;
using Cudafy.Host;
using Cudafy.Translator;

namespace Infovision.CudaLib
{
    public class QuickSort
    {
        private int[] indexTable;

        const int SEQUENTIAL_THRESHOLD = 2048;
        const int GPGPU_MAX_LEVELS = 300;
        const int GPGPU_MAX_THREADS = 128;

        public QuickSort()
        {
        }

        private void InitIndexTable(int size)
        {
            if (this.indexTable == null || this.indexTable.Length != size)
                this.indexTable = new int[size];
            
            for (int i = 0; i < size; i++)
                this.indexTable[i] = i;
        }

        public int[] GetIndexTable()
        {
            return this.indexTable;
        }

        public void CPURecursive(int[] elements)
        {
            this.InitIndexTable(elements.Length);
            QuickSort.CPURecursive(elements, this.indexTable, 0, elements.Length - 1);
        }

        public void CPUIterative(int[] elements)
        {
            this.InitIndexTable(elements.Length);
            QuickSort.CPUIterative(elements, this.indexTable, 0, elements.Length - 1);
        }

        public void CPUParalell(int[] elements)
        {
            this.InitIndexTable(elements.Length);
            QuickSort.CPUParallel(elements, this.indexTable, 0, elements.Length - 1);
        }

        public void GPUParalell(int[] elements)
        {
            this.InitIndexTable(elements.Length);

            CudafyTranslator.GenerateDebug = true; 
            CudafyModule km = CudafyTranslator.Cudafy(eArchitecture.sm_20);
            GPGPU gpu = CudafyHost.GetDevice(CudafyModes.Target);
            //GPGPU gpu = CudafyHost.GetDevice(eGPUType.Emulator);
            gpu.LoadModule(km);

            int[] elements_dev = gpu.CopyToDevice(elements);
            int[] index_dev    = gpu.CopyToDevice(indexTable);

            gpu.Launch(1, 1).GPUParalell(elements_dev, index_dev, 0, elements.Length - 1);



            gpu.CopyFromDevice(index_dev, indexTable);
            gpu.FreeAll();            
        }

        [Cudafy]
        private static void GPUParalell(GThread thread, int[] elements, int[] index, int left, int right)
        {
            int threadIndex = thread.threadIdx.x;
            int blockIndex = thread.blockIdx.x;
            int threadsPerBlock = thread.blockDim.x;
            int tickPosition = (threadIndex + (blockIndex * threadsPerBlock));

            //TODO
        }

        [Cudafy]
        public static void MergeSort(GThread thread, int[]  values, int[] results)
        {
            int[] shared = thread.AllocateShared<int>("shared");

            const unsigned int tid = thread.threadIdx.x;
            int k,u,i;
 
            // Copy input to shared mem.
            shared[tid] = values[tid];                           
    
            __syncthreads();
    
            k = 1;
            while(k < NUM)
            {
                i = 1;
                while(i+k <= NUM)
                {
                    u = i + k * 2;
                    
                    if(u > NUM)
                    {
                        u = NUM+1;
                    }
                    Merge(shared, results, i, i+k, u);
                    i = i+k*2;
                }
                k = k*2;
                __syncthreads();
            }
    
            values[tid] = shared[tid];
        }


        [Cudafy]
        public static int Merge(GThread thread, int[] values, int[] results, int l, int r, int u)
        {
            int i, j, k;
            i = l; j = r; k = l;
            while (i < r && j < u)
            {
                if (values[i] <= values[j])
                {
                    results[k] = values[i];
                    i++;
                }
                else
                {
                    results[k] = values[j];
                    j++;
                }
                k++;
            }

            while (i < r)
            {
                results[k] = values[i]; i++; k++;
            }
        }

        private static void Swap(int[] elements, int[] index, int i, int j)
        {
            int tmp = index[i];
            index[i] = index[j];
            index[j] = tmp;
        }

        private static int Partition(int[] elements, int[] index, int left, int right)
        {
            int pivotIndex = left + (right - left) / 2;
            int pivot = elements[index[pivotIndex]];
            
            QuickSort.Swap(elements, index, pivotIndex, right); //Move pivot to the end

            int storeIndex = left;                        
            for(int i = left; i < right; i++)
            {
                if (elements[index[i]] <= pivot) 
                {
                    QuickSort.Swap(elements, index, i, storeIndex);
                    storeIndex++;
                }
            }
            QuickSort.Swap(elements, index, storeIndex, right); // Move pivot to its final place

            return storeIndex;
        }

        private static void CPURecursive(int[] elements, int[] index, int left, int right)
        {
            if (left < right)
            {
                int pivotIndex = QuickSort.Partition(elements, index, left, right);

                QuickSort.CPURecursive(elements, index, left, pivotIndex - 1);
                QuickSort.CPURecursive(elements, index, pivotIndex + 1, right);
            }
        }

        private static void CPUParallel(int[] elements, int[] index, int left, int right)
        {
            //const int SEQUENTIAL_THRESHOLD = 2048;
            if (left < right)
            {
                if (right - left < SEQUENTIAL_THRESHOLD)
                {
                    QuickSort.CPURecursive(elements, index, left, right);
                }
                else
                {
                    int pivotIndex = QuickSort.Partition(elements, index, left, right);

                    Parallel.Invoke(() => QuickSort.CPUParallel(elements, index, left, pivotIndex - 1),
                                    () => QuickSort.CPUParallel(elements, index, pivotIndex + 1, right));
                }
            }    
        }

        /*
        private static void CPURecursive(int[] elements, int[] index, int left, int right)
        {
            int i = left;
            int j = right;
            int pivot = elements[index[left + (right - left) / 2]];
            while (i <= j)
            {
                while (elements[index[i]] < pivot) i++;
                while (elements[index[j]] > pivot) j--;
                if (i <= j)
                {
                    //Swap
                    int tmp = index[i];
                    index[i++] = index[j];
                    index[j--] = tmp;
                }

                if (left < j) QuickSort.CPURecursive(elements, index, left, j);
                if (i < right) QuickSort.CPURecursive(elements, index, i, right); 
            }
        }
        */ 

        private static void CPUIterative(int[] elements, int[] index, int left, int right)
        {
            int[] stack = new int[right - left + 1];
            int top = -1;
            
            stack[++top] = left;
            stack[++top] = right;

            while (top >= 0)
            {
                right = stack[top--];
                left = stack[top--];

                int pivotIndex = QuickSort.Partition(elements, index, left, right);

                if (pivotIndex - 1 > left)
                {
                    stack[++top] = left;
                    stack[++top] = pivotIndex - 1;
                }

                if (pivotIndex + 1 < right)
                {
                    stack[++top] = pivotIndex + 1;
                    stack[++top] = right;
                }
            }
        }
    }
}
