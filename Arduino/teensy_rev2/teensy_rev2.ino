#include <OctoWS2811.h>

const bool debugMode = false;

#define RED    0xFF0000
#define GREEN  0x00FF00
#define BLUE   0x0000FF
#define YELLOW 0xFFFF00
#define PINK   0xFF1088
#define ORANGE 0xE05800
#define WHITE  0xFFFFFF
#define BLACK  0x000000

const int ledsPerStrip = 170;
const int bytesPerColor = 3;
const int headerSize = 4;
const int maxRows = 8;
const int numLEDs = ledsPerStrip * maxRows;
const int maxPayloadSize = (bytesPerColor * numLEDs); 
const int maxBufSize = maxPayloadSize + headerSize;
const int baudRate = 100000000;
const unsigned long resetTime = 5000;
const int delayTime = 2000;

DMAMEM int displayMemory[ledsPerStrip*6];
int drawingMemory[ledsPerStrip*6];
const int config = WS2811_GRB | WS2811_800kHz;
OctoWS2811 leds(ledsPerStrip, displayMemory, drawingMemory, config);

byte dataACK = 0xAB; // if C# receives a 0xAB, the teensys received the data
byte displayACK = 0xCD; // if C# receives a 0xCD, the teensys sent the data to the leds

byte inputBuffer[maxBufSize];

/*

0 - 0xFFFF
1 - state
2 - payload size
3 - payload size
4 - payload
...
N - payload 

*/

int serialBufIdx;
int serialBufLen;
int * bufPtr;
bool serialPktEnd;
unsigned long startTime;
bool serialReset;
int msg_len;


void setOff();
void setWall(byte grid[]);
void setWallPtr(int * bufPtr);
void showWall();
void sendAck();
void bootScreen();
void resetScreen();
void serialInterupt();
void debugWall(int color);

void setup()
{
	leds.begin();
	leds.show();
	Serial.begin(baudRate);
	serialBufIdx = 0;
	serialBufLen = -1;
	serialPktEnd = false;
	startTime = millis();
	serialReset = false;
	msg_len = -1;

	memset(inputBuffer, 0, maxBufSize);

	bootScreen();
}

void loop()
{
	debugWall(0, WHITE);

	if(serialPktEnd)
	{
		serialBufIdx = 0;
		msg_len = -1;

		debugWall(0, RED);

		if(0xFF == inputBuffer[0])
		{
			debugWall(0, ORANGE);
			byte state = inputBuffer[1];
			switch(state)
			{
				case 0: 
					setOff();
					break;
				case 5:
					setWall(inputBuffer);
					break;
				case 7:
					showWall();
					break;
				default:
					break;
			}
		}
		serialPktEnd = false;
	}

	serialInterupt();
}

void debugWall(int ledNum, int color) 
{ 	
	if(debugMode)
	{
		leds.setPixel(ledNum, color); 
		showWall();
		delay(delayTime);
	}	
}
	
void serialEvent()
{
	if(serialPktEnd) { return; }
	if(!Serial)
	{
		Serial.begin(baudRate); 
	}

	while(Serial.available())
	{
		startTime = millis();
		inputBuffer[serialBufIdx++] = Serial.read();

		if(serialBufIdx < 4)
			continue;

		else if((msg_len == -1) && (serialBufIdx == 4))
		{
			msg_len = (inputBuffer[3] << 8) | inputBuffer[2];
		}

		if((msg_len != -1) && ((serialBufIdx - 4) == msg_len))
		{
			serialPktEnd = true;
			//sendDataAck();
			break;
		}
	}
}

void setOff()
{
	for(int i = 0; i < numLEDs; i++)
	{
		leds.setPixel(i, BLACK);
	}

	leds.show();
}

void setWallPtr(int * bufPtr)
{
	int * ptr = bufPtr;

	int r, g, b;

	for (int i = 0; i < numLEDs; i++)
	{
		if(maxPayloadSize > (ptr - bufPtr)) { return; }

		r = *ptr;
		g = *(ptr+1);
		b = *(ptr+2);

		ptr += 3;
		leds.setPixel(i, r | g | b);		
	}

	//sendAck();
	showWall();
}

void setWall(byte grid[])
{
	int r, g, b;
	int ledIdx = 0;

	for(int i = 4; i < maxBufSize; ledIdx++)
	{
		r = grid[i++] << 16;
		g = grid[i++] << 8;
		b = grid[i++];

		leds.setPixel(ledIdx, r | g | b);
	}
}

void showWall() 
{ 
	leds.show(); 
}

void sendDataAck()
{
	if(!Serial)
	{
		Serial.begin(baudRate); 
	}

	Serial.write(dataACK);
}

void sendDisplayAck()
{
	if(!Serial)
	{
		Serial.begin(baudRate); 
	}

	Serial.write(displayACK);
}

void bootScreen()
{
	for(int i = 0; i < numLEDs; i++)
	{
		leds.setPixel(i, RED);	
	}

	leds.show();
	delay(1000);

	for(int i = 0; i < numLEDs; i++)
	{
		leds.setPixel(i, YELLOW);	
	}

	leds.show();
	delay(1000);
	
	for(int i = 0; i < numLEDs; i++)
	{
		leds.setPixel(i, GREEN);	
	}

	leds.show();
	delay(1000);
}

void resetScreen()
{
	for(int i = 0; i < numLEDs; i++)
	{
		leds.setPixel(i, BLUE);	
	}

	leds.show();
}

void serialInterupt()
{
	if(abs(millis() - startTime) >= resetTime)
	{
		Serial.end();
		resetScreen();
		memset(inputBuffer, 0, maxBufSize);
		serialBufIdx = 0;
		msg_len = -1;
	}
}