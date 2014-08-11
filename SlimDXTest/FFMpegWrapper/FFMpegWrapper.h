// FFMpegWrapper.h

#pragma once

#pragma comment(lib, "avcodec.lib")
#pragma comment(lib, "avformat.lib")
#pragma comment(lib, "swscale.lib")
#pragma comment(lib, "avutil.lib")

extern "C"
{
#ifndef __STDC_CONSTANT_MACROS
#define __STDC_CONSTANT_MACROS
#endif
#include "libavcodec\avcodec.h"
#include "libavformat\avformat.h"
#include "libswscale\swscale.h"
#include "libavutil\avutil.h"
}

using namespace System;
using namespace System::Threading;
using namespace System::Runtime::InteropServices;

namespace FFMpegWrapper {

	public ref class VideoStreamDecoder
	{
	public:
		VideoStreamDecoder();
		~VideoStreamDecoder();
		!VideoStreamDecoder();
		int Initialize(String ^_fileName);
		delegate void frameDoneDelegate(IntPtr data, int width, int height, int stride);
		event frameDoneDelegate	^frameDone;
		event frameDoneDelegate	^frame2Done;
		IntPtr getCurrentFrame();
		int startDecode();
	private:
		/*
		void DecodeLeftVstThread();
		void DecodeRightVstThread();
		Thread ^leftWrkThread;
		Thread ^rightWrkThread;
		*/

		void DecoderThread();
		void SaveFrame(AVFrame *pFrame, int width, int height, int iFrame);
		String ^fileName;
		Mutex^ mut = gcnew Mutex;
		Thread^ workerThread;
		uint8_t *buffer;
		AVFrame *pFrame = NULL;
		AVFrame *pFrame2 = NULL;
		AVFrame *pCurrentRGBFrame = NULL;
		AVFrame *pCurrentRGBFrame2 = NULL;
		AVFormatContext *pFormatCtx = NULL;
		AVCodecContext *pCodecCtx = NULL;
		AVCodec *pCodec = NULL;
		int numVideoStreams = 0;
		int videoStream = -1;
	};
}
