// This is the main DLL file.

#include "stdafx.h"
#include "FFMpegWrapper.h"
#include <vcclr.h>

using namespace FFMpegWrapper;

VideoStreamDecoder::VideoStreamDecoder()
{
	avcodec_register_all();
	av_register_all();
	avformat_network_init();
}

VideoStreamDecoder::~VideoStreamDecoder()
{
	this->!VideoStreamDecoder();
}

VideoStreamDecoder::!VideoStreamDecoder()
{
	Console::Write("Garbage collect ");
	Console::WriteLine(fileName);
	workerThread->Suspend();
	av_close_input_file(pFormatCtx);
	av_free(buffer);
	av_free(pFrame);
	av_free(pCurrentRGBFrame);
	avcodec_close(pCodecCtx);
	avformat_network_deinit();
}

int VideoStreamDecoder::Initialize(String ^_fileName)
{
	fileName = _fileName;
	const char* unmngStr = (char*)(void*)Marshal::StringToHGlobalAnsi(_fileName);
	//pin_ptr<AVFormatContext*> p = &pFormatCtx;
	AVFormatContext *ppFormatCtx;
	int result = avformat_open_input(&ppFormatCtx, unmngStr, NULL, NULL);
	//int result = avformat_open_input(p, unmngStr, NULL, NULL);
	if (result >= 0)
	{
		pFormatCtx = ppFormatCtx;
		pFormatCtx->max_analyze_duration = 10000000;
	}
	else
	{
		return -1;  // Could not open file
	}
		

	if (av_find_stream_info(pFormatCtx) < 0)
		return -2; // Could not find stream info

	av_dump_format(pFormatCtx, 0, unmngStr, 0);
	numVideoStreams = pFormatCtx->nb_streams;

	for (int i = 0; i < pFormatCtx->nb_streams; i++)
	{
		if (pFormatCtx->streams[i]->codec->codec_type == AVMEDIA_TYPE_VIDEO)
		{
			videoStream = i;
			break;
		}
	}

	if (videoStream == -1)
		return -3; // Didnt find any video streams

	//AVCodecContext *pCodecCtx;

	pCodecCtx = pFormatCtx->streams[videoStream]->codec;
	pin_ptr<AVCodecContext*> pinCodecCtx = &pCodecCtx;

	//AVCodec *pCodec;
	pCodec = avcodec_find_decoder(pCodecCtx->codec_id);
	if (pCodec == NULL)
	{
		printf("Could not find any matching decoder.");
		return -4;
	}

	if (avcodec_open2(pCodecCtx, pCodec, NULL) < 0)
		return -5;

	pFrame = avcodec_alloc_frame();
	pFrame2 = avcodec_alloc_frame();
	pCurrentRGBFrame = avcodec_alloc_frame();
	pCurrentRGBFrame2 = avcodec_alloc_frame();
	if (pCurrentRGBFrame == NULL)
		return -6; //BAD

	int numberOfBytes = avpicture_get_size(PIX_FMT_BGRA, pCodecCtx->width, pCodecCtx->height);
	buffer = (uint8_t *)av_malloc(numberOfBytes * sizeof(uint8_t));

	//avpicture_fill((AVPicture *)pCurrentRGBFrame, buffer, PIX_FMT_RGB24, pCodecCtx->width, pCodecCtx->height);
	avpicture_fill((AVPicture *)pCurrentRGBFrame, buffer, PIX_FMT_BGRA, pCodecCtx->width, pCodecCtx->height);
	avpicture_fill((AVPicture *)pCurrentRGBFrame2, buffer, PIX_FMT_BGRA, pCodecCtx->width, pCodecCtx->height);

	return 1;
}

IntPtr VideoStreamDecoder::getCurrentFrame()
{
	return (IntPtr)pCurrentRGBFrame->data;
}

int VideoStreamDecoder::startDecode()
{
	workerThread = gcnew Thread(gcnew ThreadStart(this, &VideoStreamDecoder::DecoderThread));
	workerThread->Name = "Decoder thread.";
	workerThread->Start();
	return 0;
}

void VideoStreamDecoder::DecoderThread()
{
	int frameFinished;
	int frame2Finished;
	AVPacket packet;
	int i = 0;
	//while (av_read_frame(pFormatCtx, &packet) >= 0)
	while (true)
	{
		if (av_read_frame(pFormatCtx, &packet) >= 0)
		{
			if (packet.stream_index == 0)
			{
				// Decode video frame
				avcodec_decode_video2(pCodecCtx, pFrame, &frameFinished, &packet);

				if (frameFinished)
				{
					// Convert to RGB
					SwsContext *pSWSContext;
					//pSWSContext = sws_getContext(pCodecCtx->width, pCodecCtx->height, pCodecCtx->pix_fmt, pCodecCtx->width, pCodecCtx->height, PIX_FMT_RGB24, SWS_BITEXACT, 0, 0, 0);
					pSWSContext = sws_getCachedContext(NULL, pCodecCtx->width, pCodecCtx->height, pCodecCtx->pix_fmt, pCodecCtx->width, pCodecCtx->height, PIX_FMT_BGRA, SWS_BITEXACT, 0, 0, 0);
					//mut->WaitOne();
					sws_scale(pSWSContext, pFrame->data, pFrame->linesize, 0, pCodecCtx->height, pCurrentRGBFrame->data, pCurrentRGBFrame->linesize);
					//mut->ReleaseMutex();
					// Shoot event that we have a new frame
					//SaveFrame(pCurrentRGBFrame, pCodecCtx->width, pCodecCtx->height, i++);
					frameDone((IntPtr)pCurrentRGBFrame->data[0], pCodecCtx->width, pCodecCtx->height, pCurrentRGBFrame->linesize[0]);
					sws_freeContext(pSWSContext);
				}
			}
			else if (packet.stream_index == 1)
			{
				avcodec_decode_video2(pCodecCtx, pFrame2, &frame2Finished, &packet);

				if (frame2Finished)
				{
					SwsContext *pSWSContext;
					pSWSContext = sws_getCachedContext(NULL, pCodecCtx->width, pCodecCtx->height, pCodecCtx->pix_fmt, pCodecCtx->width, pCodecCtx->height, PIX_FMT_BGRA, SWS_BITEXACT, 0, 0, 0);
					//mut->WaitOne();
					sws_scale(pSWSContext, pFrame->data, pFrame->linesize, 0, pCodecCtx->height, pCurrentRGBFrame->data, pCurrentRGBFrame2->linesize);
					//mut->ReleaseMutex();
					frame2Done((IntPtr)pCurrentRGBFrame2->data[0], pCodecCtx->width, pCodecCtx->height, pCurrentRGBFrame2->linesize[0]);
					sws_freeContext(pSWSContext);
				}
			}
			av_free_packet(&packet);
		}
		Thread::Sleep(5);
	}
}

void VideoStreamDecoder::SaveFrame(AVFrame *pFrame, int width, int height, int iFrame) {
	FILE *pFile;
	char szFilename[32];
	int  y;
	printf("Saving frame frame%d.ppm\n", iFrame);
	// Open file
	sprintf(szFilename, "frame%d.ppm", iFrame);
	pFile = fopen(szFilename, "wb");
	if (pFile == NULL)
		return;

	// Write header
	fprintf(pFile, "P6\n%d %d\n255\n", width, height);

	// Write pixel data
	for (y = 0; y<height; y++)
		fwrite(pFrame->data[0] + y*pFrame->linesize[0], 1, width * 3, pFile);

	// Close file
	fclose(pFile);
}