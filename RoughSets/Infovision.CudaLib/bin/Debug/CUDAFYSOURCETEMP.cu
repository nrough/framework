
// Infovision.CudaLib.QuickSort
extern "C" __global__ void GPUParalell( int* elements, int elementsLen0,  int* index, int indexLen0, int left, int right);

// Infovision.CudaLib.QuickSort
extern "C" __global__ void GPUParalell( int* elements, int elementsLen0,  int* index, int indexLen0, int left, int right)
{
	int x = threadIdx.x;
	int x2 = blockIdx.x;
	int x3 = blockDim.x;
	int num = x + x2 * x3;
}
